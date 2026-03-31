using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class InspectionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InspectionsController> _logger;

    public InspectionsController(ApplicationDbContext context, ILogger<InspectionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Inspections (Dashboard)
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Inspection dashboard accessed by user.");
        return View(await _context.Inspections.Include(i => i.Premises).ToListAsync());
    }

    // 1. GET: Inspections/Create (Affiche le formulaire vide)
    [HttpGet]
    [Authorize(Roles = "Admin, Inspector")]
    public IActionResult Create()
    {
        ViewBag.PremisesId = new SelectList(_context.Premises, "Id", "Name");
        return View(new Inspection { InspectionDate = DateTime.Now });
    }

    // 2. POST: Inspections/Create (Enregistre les données)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin, Inspector")]
    public async Task<IActionResult> Create(Inspection inspection)
    {
        // On nettoie la validation pour les objets liés
        ModelState.Remove("Premises");
        ModelState.Remove("FollowUps");

        try
        {
            if (ModelState.IsValid)
            {
                _context.Add(inspection);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Inspection created. PremisesId: {PremisesId}, InspectionId: {InspectionId}",
                    inspection.PremisesId, inspection.Id);

                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical failure: Could not save inspection for Premises {PremisesId}", inspection.PremisesId);
            ModelState.AddModelError("", "Un problème est survenu lors de l'enregistrement.");
        }

        // Si on arrive ici, c'est qu'il y a eu une erreur : on recharge la liste
        ViewBag.PremisesId = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }


    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var inspection = await _context.Inspections.FindAsync(id);
        if (inspection != null)
        {
            // 1. On cherche s'il y a des Follow-ups liés à CETTE inspection
            var linkedFollowUps = _context.FollowUps.Where(f => f.InspectionId == id);

            // 2. On les supprime d'abord
            _context.FollowUps.RemoveRange(linkedFollowUps);

            // 3. Maintenant on peut supprimer l'inspection sans erreur SQL
            _context.Inspections.Remove(inspection);

            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Inspections/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        // On récupère l'inspection avec son local (Premises) pour l'affichage
        var inspection = await _context.Inspections
            .Include(i => i.Premises)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (inspection == null) return NotFound();

        return View(inspection);
    }

    // POST: Inspections/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionDate,Score,Notes,PremisesId")] Inspection inspection)
    {
        if (id != inspection.Id) return NotFound();

        // --- LOGIQUE MÉTIER ---
        // On met à jour l'Outcome automatiquement selon le nouveau score
        inspection.Outcome = inspection.Score >= 70 ? "Pass" : "Fail";

        // On ignore la validation de l'objet Premises (car on ne modifie pas le resto ici)
        ModelState.Remove("Premises");
        ModelState.Remove("Outcome");
        ModelState.Remove("Notes"); // AJOUTE CETTE LIGNE

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(inspection);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Inspections.Any(e => e.Id == inspection.Id)) return NotFound();
                else throw;
            }
        }
        return View(inspection);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var inspection = await _context.Inspections
            .Include(i => i.Premises) // Important pour afficher le nom du lieu
            .FirstOrDefaultAsync(m => m.Id == id);

        if (inspection == null) return NotFound();

        return View(inspection);
    }
}