using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;
    public CoursesController(ApplicationDbContext context) => _context = context;

    // INDEX
    public async Task<IActionResult> Index() => View(await _context.Courses.Include(c => c.Branch).ToListAsync());

    // CREATE
    public IActionResult Create()
    {
        ViewBag.BranchId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Branches, "Id", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Course course)
    {
        _context.Add(course);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // EDIT
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        ViewBag.BranchId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Branches, "Id", "Name", course.BranchId);
        return View(course);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Course course)
    {
        _context.Update(course);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null) _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ReleaseResults(int examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam == null) return NotFound();

        exam.ResultsReleased = true; // On publie les notes
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = exam.CourseId });
    }
}