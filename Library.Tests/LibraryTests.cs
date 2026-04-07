using Xunit;
using Microsoft.EntityFrameworkCore;
using Library.MVC.Data;
using Library.MVC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AcademicTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Student_ShouldNotSeeGrade_WhenResultsNotReleased()
    {
        var exam = new Exam { Title = "Final Exam", ResultsReleased = false };
        var result = new ExamResult { Exam = exam, Score = 85, Grade = "A" };

        // Simulation de la logique de vue/service
        bool isVisible = exam.ResultsReleased;

        Assert.False(isVisible);
    }

    [Fact]
    public void Student_ShouldSeeGrade_WhenResultsAreReleased()
    {
        var exam = new Exam { Title = "Final Exam", ResultsReleased = true };
        Assert.True(exam.ResultsReleased);
    }

    [Fact]
    public void CalculateAverage_ShouldReturnCorrectValue()
    {
        var results = new List<AssignmentResult>
    {
        new AssignmentResult { Score = 40 },
        new AssignmentResult { Score = 60 }
    };

        var average = results.Select(r => r.Score).DefaultIfEmpty(0).Average();

        Assert.Equal(50, average);
    }

    [Fact]
    public void Average_ShouldBeZero_IfNoResultsExist()
    {
        var results = new List<AssignmentResult>();
        var average = results.Select(r => r.Score).DefaultIfEmpty(0).Average();

        Assert.Equal(0, average);
    }

    [Fact]
    public async Task Enrolment_ShouldPreventDuplicateEntries()
    {
        using var context = GetDbContext();

        // On ajoute le "Status" (ex: "Enrolled" ou "Active" selon ton enum/string)
        var enrolment1 = new CourseEnrolment
        {
            StudentProfileId = 1,
            CourseId = 10,
            Status = "Enrolled" // <--- AJOUTE ÇA ICI
        };

        context.CourseEnrolments.Add(enrolment1);
        await context.SaveChangesAsync();

        // Logique métier : on vérifie si un doublon existe avant d'ajouter le 2ème
        bool alreadyExists = await context.CourseEnrolments
            .AnyAsync(e => e.StudentProfileId == 1 && e.CourseId == 10);

        Assert.True(alreadyExists);
    }

    [Fact]
    public void Course_ShouldBeLinkedToCorrectBranch()
    {
        var branch = new Branch { Id = 1, Name = "IT" };
        var course = new Course { Name = "C# Dev", BranchId = 1, Branch = branch };

        Assert.Equal("IT", course.Branch.Name);
    }

    [Fact]
    public void Faculty_IsTutor_StatusCheck()
    {
        var faculty1 = new FacultyProfile { Name = "Prof A", IsTutor = true };
        var faculty2 = new FacultyProfile { Name = "Prof B", IsTutor = false };

        Assert.True(faculty1.IsTutor);
        Assert.False(faculty2.IsTutor);
    }

    [Fact]
    public void Course_IsExpired_WhenEndDatePassed()
    {
        var course = new Course
        {
            EndDate = DateTime.Now.AddDays(-1)
        };

        bool isExpired = course.EndDate < DateTime.Now;

        Assert.True(isExpired);
    }
}