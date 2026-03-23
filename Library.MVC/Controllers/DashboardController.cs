using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.MVC.Data;
using Library.MVC.Models;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? town, string? riskRating)
    {
        var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var today = DateTime.Today;

        // Start with a queryable object for filtering
        var inspectionsQuery = _context.Inspections.AsQueryable();
        var followUpsQuery = _context.FollowUps.AsQueryable();

        // APPLY FILTERS
        if (!string.IsNullOrEmpty(town))
        {
            inspectionsQuery = inspectionsQuery.Where(i => i.Premises.Town == town);
            followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.Town == town);
        }

        if (!string.IsNullOrEmpty(riskRating))
        {
            inspectionsQuery = inspectionsQuery.Where(i => i.Premises.RiskRating == riskRating);
            followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.RiskRating == riskRating);
        }

        // AGGREGATIONS
        var viewModel = new DashboardViewModel
        {
            SelectedTown = town,
            SelectedRiskRating = riskRating,

            // Count inspections this month
            TotalInspectionsThisMonth = await inspectionsQuery
                .CountAsync(i => i.InspectionDate >= firstDayOfMonth),

            // Count failed inspections (Assuming Score < 50 means Fail)
            FailedInspectionsThisMonth = await inspectionsQuery
                .CountAsync(i => i.InspectionDate >= firstDayOfMonth && i.Score < 50),

            // Count Overdue Open Follow-ups
            OverdueOpenFollowUps = await followUpsQuery
                .CountAsync(f => f.Status == "Open" && f.DueDate < today),

            // Populate Town dropdown list from unique values in DB
            Towns = await _context.Premises.Select(p => p.Town).Distinct().ToListAsync()
        };

        return View(viewModel);
    }
}