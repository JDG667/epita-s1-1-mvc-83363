using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.MVC.Data;
using Library.MVC.Models;

public class StudentProfilesController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentProfilesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Students (Index)
    public async Task<IActionResult> Index()
    {
        return View(await _context.StudentProfiles.ToListAsync());
    }

    // GET: Students/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(m => m.Id == id);

        if (student == null) return NotFound();

        return View(student);
    }

    // GET: Students/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Students/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentProfile studentProfile)
    {
        if (ModelState.IsValid)
        {
            _context.Add(studentProfile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(studentProfile);
    }

    // GET: Students/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var student = await _context.StudentProfiles.FindAsync(id);

        if (student == null) return NotFound();

        return View(student);
    }

    // POST: Students/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentProfile student)
    {
        if (id != student.Id) return NotFound();

        if (ModelState.IsValid)
        {
            // On marque l'entité comme modifiée et on sauvegarde
            _context.Update(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(student);
    }

    // GET: Students/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(m => m.Id == id);

        if (student == null) return NotFound();

        return View(student);
    }

    // POST: Students/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _context.StudentProfiles.FindAsync(id);
        if (student != null)
        {
            _context.StudentProfiles.Remove(student);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}