using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FollowUpsController> _logger;

    public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
    {
        _context = context;
        _logger = logger;
    }


    // GET: FollowUps
    [Authorize(Roles = "Admin, Inspector, Member")]
    public async Task<IActionResult> Index()
    {
        // On récupère la liste des suivis en incluant les infos de l'inspection liée
        var followUps = await _context.FollowUps
            .Include(f => f.Inspection)
            .ThenInclude(i => i.Premises)
            .ToListAsync();

        return View(followUps);
    }

    // GET: FollowUps/Create/5
    [HttpGet]
    public IActionResult Create(int id) // 'id' doit être écrit exactement comme ça
    {

        // On crée l'objet pour la vue
        var model = new FollowUp
        {
            InspectionId = id, // On assigne l'ID reçu à notre modèle
            DueDate = DateTime.Now.AddDays(7),
            Status = "Open"
        };

        // TRÈS IMPORTANT : On passe 'model' à la vue
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin, Inspector")]
    public async Task<IActionResult> Create(FollowUp followUp)
    {
        // 1. Récupérer l'inspection actuelle pour connaître le lieu (PremisesId)
        var currentInspection = await _context.Inspections
            .Include(i => i.Premises)
            .FirstOrDefaultAsync(i => i.Id == followUp.InspectionId);

        if (currentInspection == null) return NotFound();

        // 2. REGLE : Existe-t-il déjà un Follow-up "Open" pour ce lieu ?
        var alreadyHasOpenFollowUp = await _context.FollowUps
            .AnyAsync(f => f.Status == "Open" && f.Inspection.PremisesId == currentInspection.PremisesId);

        if (alreadyHasOpenFollowUp)
        {
            ModelState.AddModelError("", "This establishment already has an active follow-up. Close the existing one before creating a new one.");
        }

        // 3. Garder ta règle sur la date (DueDate < InspectionDate)
        if (followUp.DueDate < currentInspection.InspectionDate)
        {
            ModelState.AddModelError("DueDate", "The due date cannot be earlier than the inspection date.");
        }

        ModelState.Remove("Inspection");
        ModelState.Remove("Outcome");
        ModelState.Remove("Notes");

        if (ModelState.IsValid)
        {
            followUp.Id = 0;
            _context.Add(followUp);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Inspections");
        }

        return View(followUp);
    
}

    [HttpPost]
    [Authorize(Roles = "Admin, Inspector")]
    public async Task<IActionResult> Complete(int id)
    {
        var followUp = await _context.FollowUps.FindAsync(id);

        if (followUp == null)
        {
            _logger.LogWarning("Fermeture échouée : FollowUp {Id} introuvable.", id);
            return NotFound();
        }

        // Mise à jour du statut et de la date
        followUp.Status = "Closed";
        followUp.ClosedDate = DateTime.Now; // La date d'aujourd'hui

        _context.Update(followUp);
        await _context.SaveChangesAsync();

        // LOG VALIDE POUR L'ASSIGNMENT (Event #8)
        _logger.LogInformation("Follow-up ID {Id} a été fermé avec succès le {Date}.", id, followUp.ClosedDate);

        return RedirectToAction(nameof(Index));
    }

    // GET: FollowUps/Edit/5
    [Authorize(Roles = "Admin, Inspector")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var followUp = await _context.FollowUps.FindAsync(id);
        if (followUp == null) return NotFound();

        return View(followUp);
    }

    // POST: FollowUps/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, FollowUp followUp)
    {
        if (id != followUp.Id) return NotFound();

        // On retire la validation des objets liés pour éviter les blocages
        ModelState.Remove("Inspection");

        if (ModelState.IsValid)
        {
            try
            {
                // LOGIQUE DE FERMETURE AUTOMATIQUE
                if (followUp.Status == "Closed" && followUp.ClosedDate == null)
                {
                    followUp.ClosedDate = DateTime.Now;
                }

                _context.Update(followUp);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FollowUps.Any(e => e.Id == followUp.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction("Index", "Inspections");
        }
        return View(followUp);
    }
}