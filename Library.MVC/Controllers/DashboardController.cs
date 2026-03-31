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

        var inspectionsQuery = _context.Inspections.AsQueryable();
        var followUpsQuery = _context.FollowUps.AsQueryable();

        // FILTRES
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

            // 1. Total inspections du MOIS
            TotalInspectionsThisMonth = await inspectionsQuery
                .CountAsync(i => i.InspectionDate >= firstDayOfMonth),

            // 2. Échecs du MOIS (Score < 50)
            FailedInspectionsThisMonth = await inspectionsQuery
                .CountAsync(i => i.InspectionDate >= firstDayOfMonth && i.Score < 50),

            // 3. TOUS les Overdue (Indépendant de la date de l'inspection)
            // On retire le filtre "firstDayOfMonth" ici pour avoir le total global
            OverdueOpenFollowUps = await followUpsQuery
                .CountAsync(f => f.Status == "Open" && f.DueDate < today),

            // LISTES
            Towns = await _context.Premises.Select(p => p.Town).Distinct().OrderBy(t => t).ToListAsync(),
            RiskRatings = await _context.Premises.Select(p => p.RiskRating).Distinct().OrderBy(r => r).ToListAsync()
        };

        return View(viewModel);
    }
}