using Library.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Faculty")]
public class FacultyController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public FacultyController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Index : List of courses assigned to the teacher
    public async Task<IActionResult> MyCourses()
    {
        var userId = _userManager.GetUserId(User);
        // Note: In a real app, you'd have a link table CourseFaculty. 
        // For now, let's assume Faculty can see all courses they are involved in.
        var courses = await _context.Courses.ToListAsync();
        return View(courses);
    }

    // Students by Course (Contact Details)
    public async Task<IActionResult> StudentList(int courseId)
    {
        var students = await _context.CourseEnrolments
            .Where(e => e.CourseId == courseId)
            .Include(e => e.Student)
            .Select(e => e.Student)
            .ToListAsync();

        return View(students);
    }
}