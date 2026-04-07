using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
public class StudentsController : Controller
{
    private readonly ApplicationDbContext _context;
    public StudentsController(ApplicationDbContext context) => _context = context;

    // INDEX de StudentsController
    public async Task<IActionResult> Index()
    {
        // On veut la liste des étudiants, pas les inscriptions ici !
        var students = await _context.StudentProfiles.ToListAsync();
        return View(students);
    }

    // CREATE (Note: Normalement on crée le IdentityUser d'abord, mais voici le CRUD Profil)
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(StudentProfile student)
    {
        _context.Add(student);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    // GET: Admin/Students/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var student = await _context.StudentProfiles.FindAsync(id);
        if (student == null) return NotFound();

        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentProfile student)
    {
        if (id != student.Id) return NotFound();

        // On retire les validations pour les objets complexes
        ModelState.Remove("IdentityUser");
        ModelState.Remove("Enrolments");
        ModelState.Remove("AssignmentResults");
        ModelState.Remove("ExamResults");

        if (ModelState.IsValid)
        {
            try
            {
                // 1. Récupérer l'entité originale en base (SANS suivi pour éviter les conflits)
                var studentInDb = await _context.StudentProfiles.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

                if (studentInDb == null) return NotFound();

                // 2. Réaffecter les valeurs qui NE SONT PAS dans le formulaire 
                // au cas où elles seraient arrivées nulles
                student.StudentNumber = studentInDb.StudentNumber;
                student.IdentityUserId = studentInDb.IdentityUserId;

                // 3. Maintenant on update
                _context.Update(student);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.StudentProfiles.Any(e => e.Id == student.Id)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                // Affiche l'erreur réelle si la base de données rejette le SQL
                ModelState.AddModelError("", "Erreur SQL : " + ex.Message);
            }
        }

        // Si on arrive ici, c'est qu'il y a un souci de validation
        return View(student);
    }



// DELETE
public async Task<IActionResult> Delete(int id)
    {
        var student = await _context.StudentProfiles.FindAsync(id);
        if (student != null) _context.StudentProfiles.Remove(student);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


}

