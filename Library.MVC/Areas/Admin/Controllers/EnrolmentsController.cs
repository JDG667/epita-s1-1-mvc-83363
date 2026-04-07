using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
public class EnrolmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    public EnrolmentsController(ApplicationDbContext context) => _context = context;

    // Dans EnrolmentsController.cs
    public async Task<IActionResult> Index()
    {
        return View(await _context.CourseEnrolments
            .Include(e => e.Student) // Sans ça, le nom de l'étudiant est NULL
            .Include(e => e.Course)  // Sans ça, le nom du cours est NULL
            .ToListAsync());
    }

    // CREATE
    public IActionResult Create()
    {
        ViewBag.StudentProfileId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.StudentProfiles, "Id", "Name");
        ViewBag.CourseId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Courses, "Id", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CourseEnrolment enrol)
    {
        enrol.EnrolDate = DateTime.Now;
        _context.Add(enrol);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    [HttpPost] // Ta vue utilise un formulaire POST pour la suppression
    public async Task<IActionResult> Delete(int id)
    {
        var enrol = await _context.CourseEnrolments.FindAsync(id);
        if (enrol != null) _context.CourseEnrolments.Remove(enrol);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Enrolments/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var enrolment = await _context.CourseEnrolments.FindAsync(id);
        if (enrolment == null) return NotFound();

        // On remplit les listes pour les menus déroulants
        ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", enrolment.CourseId);
        ViewData["StudentProfileId"] = new SelectList(_context.StudentProfiles, "Id", "Name", enrolment.StudentProfileId);

        return View(enrolment);
    }

    // POST: Admin/Enrolments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,StudentProfileId,EnrolmentDate")] CourseEnrolment enrolment)
    {
        if (id != enrolment.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(enrolment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrolmentExists(enrolment.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(enrolment);
    }

    private bool EnrolmentExists(int id) => _context.CourseEnrolments.Any(e => e.Id == id);
}
