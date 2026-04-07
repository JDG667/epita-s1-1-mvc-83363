using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.MVC.Data;

namespace Library.MVC.Areas.Student.Controllers
{

    [Area("Student")]
    [Authorize(Roles = "Student")] // SEULS les étudiants entrent ici
    public class PortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PortalController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 1. On récupère l'ID de l'utilisateur connecté
            var userId = _userManager.GetUserId(User);

            // 2. On récupère son profil et ses inscriptions
            var profile = await _context.StudentProfiles
                .Include(p => p.Enrolments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (profile == null) return NotFound("Profil non trouvé.");

            return View(profile);
        }
    }

}