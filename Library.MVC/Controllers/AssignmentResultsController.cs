using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin, Faculty")]
public class AssignmentResultsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AssignmentResultsController(ApplicationDbContext context) => _context = context;

    // Affiche le formulaire de saisie pour un devoir spécifique
    public async Task<IActionResult> CreateAssignmentResult(int assignmentId)
    {
        var assignment = await _context.Assignments.FindAsync(assignmentId);
        ViewBag.AssignmentTitle = assignment?.Title;
        ViewBag.AssignmentId = assignmentId;

        // Liste des étudiants pour le menu déroulant
        ViewBag.StudentProfileId = new SelectList(_context.StudentProfiles, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAssignmentResult(AssignmentResult result)
    {
        if (ModelState.IsValid)
        {
            _context.AssignmentResults.Add(result);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Assignments");
        }
        return View(result);
    }

    // Publication des résultats d'un examen (Admin uniquement)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Release(int examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam != null)
        {
            exam.ResultsReleased = true;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index", "Exams");
    }

    public async Task<IActionResult> Index(int? courseId)
    {
        var results = _context.AssignmentResults
            .Include(r => r.Student)
            .Include(r => r.Assignment);

        if (courseId.HasValue)
        {
            // Filter results only for the selected course
            return View(await results.Where(r => r.Assignment.CourseId == courseId).ToListAsync());
        }

        return View(await results.ToListAsync());
    }
}