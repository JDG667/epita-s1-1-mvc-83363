using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.MVC.Data;
using Library.MVC.Models;



namespace Library.MVC.Controllers
{
    [Authorize]
    public class PremisesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PremisesController> _logger;

        public PremisesController(ApplicationDbContext context, ILogger<PremisesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index() => View(await _context.Premises.ToListAsync());

        [Authorize(Roles = "Admin, Inspector")]
        public IActionResult Create()
        {
            
            var model = new Premises();
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin, Inspector")]
        public async Task<IActionResult> Create(Premises premises)
        {
            if (ModelState.IsValid)
            {
                _context.Add(premises);
                await _context.SaveChangesAsync();
                // LOG 1: Information - Création de local
                _logger.LogInformation("Premises created. Name: {Name}, Town: {Town}", premises.Name, premises.Town);
                return RedirectToAction(nameof(Index));
            }
            return View(premises);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. On récupère le local
            var premises = await _context.Premises.FindAsync(id);

            if (premises != null)
            {
                // 2. On trouve toutes les inspections liées à ce local
                var linkedInspections = _context.Inspections.Where(i => i.PremisesId == id).ToList();
                var inspectionIds = linkedInspections.Select(i => i.Id).ToList();

                // 3. ON SUPPRIME LES PETITS-ENFANTS (Follow-ups)
                // On cherche tous les suivis qui appartiennent aux inspections de ce local
                var linkedFollowUps = _context.FollowUps
                    .Where(f => inspectionIds.Contains(f.InspectionId));

                _context.FollowUps.RemoveRange(linkedFollowUps);

                // 4. ON SUPPRIME LES PARENTS (Inspections)
                _context.Inspections.RemoveRange(linkedInspections);

                // 5. ON SUPPRIME LE GRAND-PÈRE (Premises)
                _context.Premises.Remove(premises);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}