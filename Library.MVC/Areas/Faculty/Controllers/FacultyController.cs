using Library.MVC.Data;
using Library.MVC.Models; // Assure-toi que GradebookViewModel est ici
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Areas.Faculty.Controllers
{
    [Area("Faculty")]
    public class FacultyController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FacultyController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Gradebook(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Assignments).ThenInclude(a => a.Results)
                .Include(c => c.Exams).ThenInclude(e => e.Results)
                .Include(c => c.Enrolments)
                    .ThenInclude(e => e.Student) // <--- ESSAYE "Student" ICI au lieu de StudentProfile
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var viewModel = new GradebookViewModel
            {
                Course = course,
                // On ne prend que les étudiants liés via la table de liaison Enrolments
                Students = course.Enrolments.Select(e => e.Student).ToList(),
                Assignments = course.Assignments.ToList(),
                Exams = course.Exams.ToList()
            };

            return View(viewModel);


        }


        // 1. Afficher le formulaire de modification
        [HttpGet]
        public async Task<IActionResult> EditExamResult(int examId, int studentId)
        {
            var result = await _context.ExamResults
                .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentProfileId == studentId);

            if (result == null) return NotFound();

            return View(result);
        }

        // 2. Enregistrer la modification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExamResult(ExamResult model)
        {
            if (ModelState.IsValid)
            {
                var result = await _context.ExamResults.FindAsync(model.Id);
                if (result == null) return NotFound();

                // Mise à jour des données
                result.Score = model.Score;
                result.Grade = model.Grade; // Ex: "A", "B", etc.

                _context.Update(result);
                await _context.SaveChangesAsync();

                // Retour au Gradebook du cours
                var exam = await _context.Exams.FindAsync(model.ExamId);
                return RedirectToAction("Gradebook", new { id = exam.CourseId });
            }
            return View(model);
        }
    }
}