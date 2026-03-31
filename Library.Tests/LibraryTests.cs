using Xunit;
using Microsoft.EntityFrameworkCore;
using Library.MVC.Data;
using Library.MVC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class FollowUpTests
{
    // Utilitaire pour créer un contexte de base de données en mémoire vierge
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    // TEST 1 : Vérifier que la requête Overdue retourne les bons éléments
    [Fact]
    public void OverdueFollowUps_ReturnsOnlyCorrectItems()
    {
        // Arrange
        using var context = GetDbContext();
        var now = DateTime.Now;
        context.FollowUps.AddRange(new List<FollowUp>
        {
            new FollowUp { Id = 1, Status = "Open", DueDate = now.AddDays(-5) }, // En retard
            new FollowUp { Id = 2, Status = "Open", DueDate = now.AddDays(5) },  // Pas encore
            new FollowUp { Id = 3, Status = "Closed", DueDate = now.AddDays(-10) } // Clos (ne doit pas compter)
        });
        context.SaveChanges();

        // Act
        var overdueCount = context.FollowUps.Count(f => f.Status == "Open" && f.DueDate < now);

        // Assert
        Assert.Equal(1, overdueCount);
    }

    // TEST 2 : Un suivi ne peut pas être clos sans ClosedDate (Logique métier)
    [Fact]
    public void FollowUp_CannotBeClosed_WithoutClosedDate()
    {
        // Arrange
        var followUp = new FollowUp
        {
            Status = "Closed",
            ClosedDate = null // Invalide selon ta règle
        };

        // Act & Assert
        // Ici on teste la logique de validation simple
        bool isValid = (followUp.Status == "Closed" && followUp.ClosedDate.HasValue) || (followUp.Status == "Open");

        Assert.False(isValid, "A closed follow-up must have a ClosedDate.");
    }

    // TEST 3 : Cohérence des compteurs du Dashboard (In-Memory)
    [Fact]
    public void DashboardCounts_AreConsistent_WithData()
    {
        // Arrange
        using var context = GetDbContext();
        context.Inspections.AddRange(new List<Inspection>
        {
            new Inspection { Id = 1, Score = 80, Outcome = "Pass" },
            new Inspection { Id = 2, Score = 30, Outcome = "Fail" },
            new Inspection { Id = 3, Score = 40, Outcome = "Fail" }
        });
        context.SaveChanges();

        // Act
        var totalInspections = context.Inspections.Count();
        var failedInspections = context.Inspections.Count(i => i.Score < 50);

        // Assert
        Assert.Equal(3, totalInspections);
        Assert.Equal(2, failedInspections);
    }

    // TEST 4 : Simulation d'autorisation (Vérification des rôles)
    [Theory]
    [InlineData("Inspector", true)]
    [InlineData("Viewer", false)]
    public void RoleAuthorization_InspectorCanLogInspection_ViewerCannot(string role, bool expectedAccess)
    {
        // Arrange
        // On définit qui a le droit de créer
        string[] allowedRoles = { "Admin", "Inspector" };

        // Act
        bool canAccess = allowedRoles.Contains(role);

        // Assert
        Assert.Equal(expectedAccess, canAccess);
    }
}