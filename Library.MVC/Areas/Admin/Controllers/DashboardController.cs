using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Areas.Admin.Controllers
{
    [Area("Admin")] // INDISPENSABLE
    [Authorize(Roles = "Admin")] // Sécurité serveur
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}