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
        followUp.Id = 0;

        // --- ÉTAPE CRUCIALE : On retire les champs qui bloquent la validation ---
        ModelState.Remove("Inspection"); // On ne valide pas l'objet complet, seulement l'ID
        ModelState.Remove("Outcome");    // On autorise le vide à la création
        ModelState.Remove("Notes");      // On autorise le vide à la création

        if (ModelState.IsValid)
        {
            _context.Add(followUp);
            await _context.SaveChangesAsync();

            // Redirection vers l'index des inspections après le succès
            return RedirectToAction("Index", "Inspections");
        }

        // Si on arrive ici, c'est que le formulaire est encore invalide.
        // Les erreurs s'afficheront grâce au asp-validation-summary dans ta vue.
        return View(followUp);
    }

    // POST: FollowUps/Complete/5
    [HttpPost]
    [Authorize(Roles = "Admin, Inspector")]
    public async Task<IActionResult> Complete(int id)
    {
        var followUp = await _context.FollowUps.FindAsync(id);
        if (followUp != null)
        {
            // Logic to close the case
            // LOG EVENT #8: Status Change/Workflow Completion
            _logger.LogInformation("Follow-up ID {Id} has been successfully closed/completed.", id);
        }
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