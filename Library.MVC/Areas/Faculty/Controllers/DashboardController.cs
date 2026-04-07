using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Faculty")]
[Authorize(Roles = "Faculty")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var faculty = await _context.FacultyProfiles.FirstOrDefaultAsync(f => f.IdentityUserId == userId);

        if (faculty == null) return NotFound();

        ViewBag.IsTutor = faculty.IsTutor;

        // Récupération des cours du prof
        var myCourses = await _context.Courses
            .Include(c => c.Branch)
            .Include(c => c.Enrolments)
                .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.AssignmentResults)
                        .ThenInclude(r => r.Assignment)
            .Where(c => c.FacultyProfileId == faculty.Id)
            .ToListAsync();

        return View(myCourses);
    }

    // Affiche le tableau complet pour un cours précis
    public async Task<IActionResult> Gradebook(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Assignments).ThenInclude(a => a.Results)
            .Include(c => c.Exams).ThenInclude(e => e.Results)
            .Include(c => c.Enrolments).ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        var viewModel = new GradebookViewModel
        {
            Course = course,
            Students = course.Enrolments.Select(e => e.Student).ToList(),
            Assignments = course.Assignments.ToList(),
            Exams = course.Exams.ToList()
        };

        return View(viewModel);
    }

    // --- PARTIE EXAMENS ---

    // 1. Affiche le formulaire (GET)
    [HttpGet]
    public async Task<IActionResult> EditExamResult(int id)
    {
        var result = await _context.ExamResults
            .Include(r => r.Exam)
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (result == null) return NotFound();

        return View(result); // Cherche Views/Dashboard/EditExamResult.cshtml
    }

    // 2. Sauvegarde les modifs (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditExamResult(ExamResult model)
    {
        var dbResult = await _context.ExamResults.FirstOrDefaultAsync(r => r.Id == model.Id);
        if (dbResult == null) return NotFound();

        dbResult.Score = model.Score;
        dbResult.Grade = model.Grade;

        _context.Update(dbResult);
        await _context.SaveChangesAsync();

        // Récupère l'ID du cours pour revenir au Gradebook
        var exam = await _context.Exams.FindAsync(dbResult.ExamId);
        return RedirectToAction("Gradebook", new { id = exam.CourseId });
    }

    // GET: Afficher le formulaire pour un Devoir
    [HttpGet]
    public async Task<IActionResult> EditAssignmentResult(int id)
    {
        var result = await _context.AssignmentResults
            .Include(r => r.Assignment)
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (result == null) return NotFound();

        return View(result); // Cherchera Views/Dashboard/EditAssignmentResult.cshtml
    }

    // POST: Sauvegarder la note du Devoir
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAssignmentResult(AssignmentResult model)
    {
        var dbResult = await _context.AssignmentResults.FirstOrDefaultAsync(r => r.Id == model.Id);
        if (dbResult == null) return NotFound();

        dbResult.Score = model.Score;

        _context.Update(dbResult);
        await _context.SaveChangesAsync();

        // Retour au Gradebook du cours
        var assignment = await _context.Assignments.FindAsync(dbResult.AssignmentId);
        return RedirectToAction("Gradebook", new { id = assignment.CourseId });
    }
}