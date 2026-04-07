using Library.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context) => _context = context;

    // 1. Manage Enrolments (Simple List)
    public async Task<IActionResult> Enrolments()
    {
        var enrolments = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ToListAsync();
        return View(enrolments);
    }

    // 2. Manage Result Release (Toggle)
    public async Task<IActionResult> ExamStatus()
    {
        var exams = await _context.Exams.Include(e => e.Course).ToListAsync();
        return View(exams);
    }

    [HttpPost]
    public async Task<IActionResult> ReleaseResults(int examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam != null)
        {
            // Simple toggle logic
            exam.ResultsReleased = !exam.ResultsReleased;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ExamStatus));
    }
}