using Bogus;
using Library.MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.EnsureCreated();

            // --- 1. ROLES ---
            string[] roles = { "Admin", "Faculty", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // --- 2. ADMIN ---
            var adminEmail = "admin@uni.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // --- 3. BRANCHES ---
            if (!context.Branches.Any())
            {
                context.Branches.AddRange(new List<Branch> {
                    new Branch { Name = "Dublin Main Campus", Address = "101 O'Connell St, Dublin" },
                    new Branch { Name = "Cork Branch", Address = "202 Lee Road, Cork" },
                    new Branch { Name = "Galway Tech Hub", Address = "303 Atlantic Way, Galway" }
                });
                await context.SaveChangesAsync();
            }
            var branches = await context.Branches.ToListAsync();

            // --- 4. FACULTY ---
            if (!context.FacultyProfiles.Any())
            {
                var faker = new Faker();
                for (int i = 1; i <= 3; i++)
                {
                    var email = $"faculty{i}@uni.com";
                    if (await userManager.FindByEmailAsync(email) == null)
                    {
                        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                        var result = await userManager.CreateAsync(user, "Faculty123!");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Faculty");
                            context.FacultyProfiles.Add(new FacultyProfile
                            {
                                IdentityUserId = user.Id,
                                Name = $"Professor {faker.Name.LastName()}",
                                Email = email,
                                Phone = $"087-123-456{i}",
                                IsTutor = (i == 1)
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
            var allFaculty = await context.FacultyProfiles.ToListAsync();

            // --- 5. COURSES ---
            if (!context.Courses.Any())
            {
                int facultyIndex = 0;
                foreach (var b in branches)
                {
                    context.Courses.AddRange(new List<Course>
                    {
                        new Course { Name = $"Web Dev ({b.Name})", BranchId = b.Id, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(6), FacultyProfileId = allFaculty[facultyIndex % allFaculty.Count].Id },
                        new Course { Name = $"Data Science ({b.Name})", BranchId = b.Id, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(8), FacultyProfileId = allFaculty[(facultyIndex + 1) % allFaculty.Count].Id },
                        new Course { Name = $"Cyber Sec ({b.Name})", BranchId = b.Id, StartDate = DateTime.Now.AddMonths(1), EndDate = DateTime.Now.AddMonths(7), FacultyProfileId = allFaculty[(facultyIndex + 2) % allFaculty.Count].Id }
                    });
                    facultyIndex++;
                }
                await context.SaveChangesAsync();
            }
            var allCourses = await context.Courses.ToListAsync();

            // --- 6. STUDENTS + ENROLMENTS (ALIGNED BY BRANCH) ---
            if (!context.StudentProfiles.Any())
            {
                var faker = new Faker("en");
                for (int i = 1; i <= 10; i++)
                {
                    var firstName = faker.Name.FirstName();
                    var lastName = faker.Name.LastName();
                    var email = $"{firstName.ToLower()}.{lastName.ToLower()}@uni.com";

                    var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                    var result = await userManager.CreateAsync(user, "Student123!");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Student");

                        // Déterminer la branche de l'étudiant (pour cohérence)
                        var studentBranch = branches[faker.Random.Number(0, branches.Count - 1)];

                        var profile = new StudentProfile
                        {
                            IdentityUserId = user.Id,
                            Name = $"{firstName} {lastName}",
                            Email = email,
                            StudentNumber = 202600 + i,
                            DateOfBirth = faker.Date.Past(10, DateTime.Now.AddYears(-18)),
                            Address = faker.Address.FullAddress(),
                            Phone = faker.Phone.PhoneNumber("08#-###-####")
                        };
                        context.StudentProfiles.Add(profile);
                        await context.SaveChangesAsync();

                        // FILTRE : On n'inscrit l'élève qu'aux cours de SA branche
                        var availableCourses = allCourses.Where(c => c.BranchId == studentBranch.Id).ToList();
                        var myCourses = availableCourses.OrderBy(x => Guid.NewGuid()).Take(Math.Min(2, availableCourses.Count)).ToList();

                        foreach (var c in myCourses)
                        {
                            context.CourseEnrolments.Add(new CourseEnrolment
                            {
                                StudentProfileId = profile.Id,
                                CourseId = c.Id,
                                EnrolDate = DateTime.Now.AddDays(-faker.Random.Number(1, 30)),
                                Status = "Confirmed"
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();

                // --- 7. DATA GENERATION (ATTENDANCE, ASSIGNMENTS, EXAMS) ---
                var enrolments = await context.CourseEnrolments.ToListAsync();
                var fakerData = new Faker();

                foreach (var enrolment in enrolments)
                {
                    // A. Attendance
                    for (int j = 0; j < 5; j++)
                    {
                        context.AttendanceRecords.Add(new AttendanceRecord
                        {
                            CourseEnrolmentId = enrolment.Id,
                            Date = DateTime.Now.AddDays(-(j * 7)).Date, // Une fois par semaine
                            Present = fakerData.Random.Bool(0.85f)
                        });
                    }

                    // B. Assignments
                    var assignment = await context.Assignments.FirstOrDefaultAsync(a => a.CourseId == enrolment.CourseId);
                    if (assignment == null)
                    {
                        assignment = new Assignment
                        {
                            CourseId = enrolment.CourseId,
                            Title = "Project Alpha",
                            MaxScore = 100,
                            DueDate = DateTime.Now.AddDays(14)
                        };
                        context.Assignments.Add(assignment);
                        await context.SaveChangesAsync();
                    }

                    context.AssignmentResults.Add(new AssignmentResult
                    {
                        AssignmentId = assignment.Id,
                        StudentProfileId = enrolment.StudentProfileId,
                        Score = fakerData.Random.Number(40, 100),
                        Feedback = "Well structured work."
                    });

                    // C. Exams
                    var exam = await context.Exams.FirstOrDefaultAsync(e => e.CourseId == enrolment.CourseId);
                    if (exam == null)
                    {
                        exam = new Exam
                        {
                            CourseId = enrolment.CourseId,
                            Title = "Final Exam",
                            Date = DateTime.Now.AddMonths(1),
                            MaxScore = 100,
                            ResultsReleased = true
                        };
                        context.Exams.Add(exam);
                        await context.SaveChangesAsync();
                    }

                    int score = fakerData.Random.Number(35, 100);
                    context.ExamResults.Add(new ExamResult
                    {
                        ExamId = exam.Id,
                        StudentProfileId = enrolment.StudentProfileId,
                        Score = score,
                        Grade = score >= 70 ? "A" : score >= 55 ? "B" : score >= 40 ? "C" : "D"
                    });
                }
                await context.SaveChangesAsync();
            }
        }
    }
}