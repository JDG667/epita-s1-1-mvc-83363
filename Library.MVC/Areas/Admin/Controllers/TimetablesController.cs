using Library.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Areas.Admin.Controllers
{ // 1. Le namespace doit être complet

    [Area("Admin")]
    public class TimetablesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TimetablesController(ApplicationDbContext context) => _context = context;

        // Cette méthode manquante est la raison de ton 404 sur /Admin/Timetables
        public async Task<IActionResult> Index()
        {
            // On récupère les inscriptions avec les cours et les profils étudiants
            var data = await _context.CourseEnrolments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .ToListAsync();

            return View(data);
        }

        // Affiche l'emploi du temps global ou par étudiant
        public async Task<IActionResult> StudentSchedule(int studentId)
        {
            // On cherche les inscriptions de l'étudiant et on inclut les horaires des cours
            var schedule = await _context.CourseEnrolments
                .Where(e => e.StudentProfileId == studentId)
                .Include(e => e.Course)
                //.ThenInclude(c => c.Timetables) // Nécessite d'ajouter List<Timetable> dans le modèle Course
                .ToListAsync();

            return View(schedule);
        }
    }
}