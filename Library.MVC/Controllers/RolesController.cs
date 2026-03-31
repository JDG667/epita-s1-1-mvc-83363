using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace Library.MVC.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // GET: /Admin/Roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles); // Utilise Index.cshtml pour tout
        }

        // POST: /Admin/Roles/Create (form intégré dans Index.cshtml)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return RedirectToAction(nameof(Index));

            roleName = roleName.Trim();

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    // AU LIEU DE REDIRIGER : On renvoie la vue Index avec la liste actuelle
                    var roles = _roleManager.Roles.ToList();
                    return View("Index", roles);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Roles/Delete (form intégré dans Index.cshtml)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(Index));

            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}