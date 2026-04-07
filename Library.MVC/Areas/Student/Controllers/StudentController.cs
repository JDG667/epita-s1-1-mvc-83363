using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.MVC.Data;
using System.Security.Claims;
using Library.MVC.Models;

namespace Library.MVC.Areas.Student.Controllers
{

    [Area("Student")]
    [Authorize(Roles = "Student")] // SEULS les étudiants entrent ici
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        public async Task<IActionResult> MyGrades()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var student = await _context.StudentProfiles
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.Assignments)
                            .ThenInclude(a => a.Results) // On charge sans filtrer ici
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.Exams)
                            .ThenInclude(ex => ex.Results) // On charge sans filtrer ici
                .FirstOrDefaultAsync(s => s.IdentityUserId == userId);

            if (student == null) return NotFound();

            var viewModel = new StudentGradebookViewModel
            {
                Student = student,
                Enrolments = student.Enrolments.ToList()
            };

            return View(viewModel);
        }
    }

}