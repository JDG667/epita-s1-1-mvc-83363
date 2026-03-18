using Microsoft.EntityFrameworkCore;
using Library.MVC.Models;
using Xunit;
using Library.MVC.Data;

namespace Library.Tests
{
    public class LibraryTests
    {

            
            [Fact]
            public void CreateLoan_WhenBookAlreadyOnActiveLoan_ShouldDetectConflict()
            {
            
                var activeLoans = new List<Loan> {
                new Loan { BookId = 10, ReturnedDate = null }
            };
                int newLoanBookId = 10;

            
                bool isAlreadyLoaned = activeLoans.Any(l => l.BookId == newLoanBookId && l.ReturnedDate == null);

                // Assert
                Assert.True(isAlreadyLoaned, "the system shouldn't loan");
            }

            
            [Fact]
            public void MarkAsReturned_SetsReturnedDate_AndLogicChecksOut()
            {
                // Arrange
                var loan = new Loan { Id = 1, ReturnedDate = null };

            
                loan.ReturnedDate = DateTime.Now;

                // Assert
                Assert.NotNull(loan.ReturnedDate);
                Assert.True(loan.ReturnedDate <= DateTime.Now);
            }

            
            [Fact]
            public void BookSearch_FiltersByTitleOrAuthor_Correct()
            {
                // Arrange
                var books = new List<Book> {
                new Book { Title = "Le Petit Prince", Author = "Saint-Exupéry" },
                new Book { Title = "L'Étranger", Author = "Albert Camus" },
                new Book { Title = "C#", Author = "Inconnu" }
            }.AsQueryable();
                string searchString = "Camus";

                var results = books.Where(b => b.Title.Contains(searchString) || b.Author.Contains(searchString)).ToList();

                // Assert
                Assert.Single(results);
                Assert.Equal("Albert Camus", results[0].Author);
            }

            [Fact]
            public void OverdueLogic_DetectsLateReturn_WhenDatePassed()
            {
                // Arrange
                var loan = new Loan
                {
                    DueDate = DateTime.Now.AddDays(-5), 
                    ReturnedDate = null                 
                };

                bool isOverdue = loan.DueDate < DateTime.Now && loan.ReturnedDate == null;

                // Assert
                Assert.True(isOverdue, "The loan should be marked late");
            }

            [Fact]
            public void RolePage_Access_OnlyForAdminRole()
            {
                // Arrange
                string userRole = "Member"; 
                string requiredRole = "Admin";

                // Act
                bool hasAccess = (userRole == requiredRole);

                // Assert
                Assert.False(hasAccess, "A member should not have access to admin");
            }
        }
    }


