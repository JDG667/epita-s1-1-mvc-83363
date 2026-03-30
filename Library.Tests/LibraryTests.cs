using Microsoft.EntityFrameworkCore;
using Library.MVC.Models;
using Library.MVC.Data;
using Xunit;

namespace Library.Tests
{
    public class LibraryTests
    {
        // 1. LA MÉTHODE VA ICI (C'est une méthode privée utilitaire)
        private ApplicationDbContext GetDatabaseContext()
        {
            // Remplace par ta chaîne de connexion SQL Server
            // Pointe bien vers la base de TEST, pas la base de DEV
            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=Library_TestDB;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            var context = new ApplicationDbContext(options);

            // CRITIQUE : Supprime et recrée la base à chaque test 
            // pour être sûr de repartir de zéro (Isolation)
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        // 2. TES TESTS UTILISENT CETTE MÉTHODE
        [Fact]
        public void Test_Dashboard_Counts_Consistent()
        {
            // On appelle la méthode pour avoir une base toute neuve
            using var context = GetDatabaseContext();

            // Arrange
            context.Premises.Add(new Premises { Name = "Test Restaurant", Town = "Montreal" });
            context.SaveChanges();

            // Act
            var count = context.Premises.Count();

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public void Test_FollowUp_Overdue_Logic()
        {
            using var context = GetDatabaseContext();

            // Arrange
            var fu = new FollowUp { DueDate = DateTime.Now.AddDays(-1), Status = "Pending" };
            context.FollowUps.Add(fu);
            context.SaveChanges();

            // Act
            var isOverdue = fu.DueDate < DateTime.Now && fu.Status != "Closed";

            // Assert
            Assert.True(isOverdue);
        }
    }
}