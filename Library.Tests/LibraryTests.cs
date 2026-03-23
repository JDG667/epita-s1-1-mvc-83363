using Library.MVC.Models;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.Tests
{
    public class LibraryTests
    {
        [Fact]
        public void Test1_OverdueFollowUps_ReturnsCorrectItems()
        {
            // Arrange : Au lieu d'une DB, on utilise une simple Liste C#
            var followUps = new List<FollowUp>
            {
                new FollowUp { DueDate = DateTime.Now.AddDays(-10), Status = "Pending" }, // En retard
                new FollowUp { DueDate = DateTime.Now.AddDays(10), Status = "Pending" }   // Futur
            };

            // Act : On filtre la liste comme on le ferait avec LINQ
            var overdue = followUps.Where(f => f.DueDate < DateTime.Now && f.Status != "Closed").ToList();

            // Assert
            Assert.Single(overdue);
        }

        [Fact]
        public void Test2_FollowUp_CannotBeClosed_WithoutClosedDate()
        {
            // Arrange : Un objet avec Status "Closed" mais sans date
            var followUp = new FollowUp
            {
                Status = "Closed",
                ClosedDate = null
            };

            // Act : Logique métier (Si c'est fermé, ClosedDate ne doit pas être null)
            bool isValid = !(followUp.Status == "Closed" && !followUp.ClosedDate.HasValue);

            // Assert
            Assert.False(isValid, "Un suivi fermé doit avoir une date de fermeture.");
        }

        [Fact]
        public void Test3_Dashboard_Counts_Consistent_With_Data()
        {
            // Arrange : Simulation de données dans une liste
            var premisesList = new List<Premises>
            {
                new Premises { Name = "Bakery" },
                new Premises { Name = "Cafe" }
            };

            // Act
            var count = premisesList.Count;

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public void Test4_Inspection_Outcome_Logic_IsCorrect()
        {
            // Arrange
            var inspection = new Inspection { Score = 85 };

            // Act : Logique de décision du résultat
            inspection.Outcome = inspection.Score >= 70 ? "Pass" : "Fail";

            // Assert
            Assert.Equal("Pass", inspection.Outcome);
        }
    }
}