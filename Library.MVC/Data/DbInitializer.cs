using Bogus;
using Library.MVC.Models;
using Library.MVC.Data;

namespace Library.MVC.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {

            if (context.Loans.Any()) context.Loans.RemoveRange(context.Loans);
            if (context.Books.Any()) context.Books.RemoveRange(context.Books);
            if (context.Members.Any()) context.Members.RemoveRange(context.Members);

            context.SaveChanges();


            if (!context.Books.Any())
            {
                var bookFaker = new Faker<Book>()
                    .RuleFor(b => b.Title, f => f.Lorem.Sentence(3).TrimEnd('.'))
                    .RuleFor(b => b.Author, f => f.Name.FullName())
                    .RuleFor(b => b.Isbn, f => f.Random.Replace("###-##########"))
                    .RuleFor(b => b.Category, f => f.PickRandom(new[] {
                        "Fiction",
                        "Science Fiction",
                        "Fantasy",
                        "Thriller",
                        "History",
                        "Biography",
                        "Science",
                        "Technology",
                        "Arts",
                        "Children",
                        "Graphic Novels"
                    }));

                context.Books.AddRange(bookFaker.Generate(20));
                context.SaveChanges();
            }

            if (!context.Members.Any())
            {
                var memberFaker = new Faker<Member>()
                    .RuleFor(m => m.FullName, f => f.Name.FullName())
                    .RuleFor(m => m.Email, f => f.Internet.Email())
                    .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber("###-###-####"));

                context.Members.AddRange(memberFaker.Generate(10));
                context.SaveChanges();
            }

     
            if (!context.Loans.Any())
            {
                var books = context.Books.ToList();
                var members = context.Members.ToList();
                var fixedNow = new DateTime(2026, 3, 15, 12, 0, 0);

                var loanFaker = new Faker<Loan>()
                    .UseSeed(1337)
                    .UseDateTimeReference(fixedNow)
                    .RuleFor(l => l.BookId, f => f.PickRandom(books).Id)
                    .RuleFor(l => l.MemberId, f => f.PickRandom(members).Id)
                    .RuleFor(l => l.LoanDate, f => f.Date.Recent(30))
                    .RuleFor(l => l.DueDate, (f, l) => l.LoanDate.AddDays(14))
                    .RuleFor(l => l.ReturnedDate, (f, l) =>
                    {
                        bool isPastDue = fixedNow > l.DueDate;

                        if (isPastDue)
                        {
                            return f.Random.Bool(0.8f) ? f.Date.Between(l.LoanDate, l.DueDate) : null;
                        }
                        else
                        {
                            return f.Random.Bool(0.2f) ? f.Date.Between(l.LoanDate, fixedNow) : null;
                        }
                    });

                var generatedLoans = loanFaker.Generate(25);
                context.Loans.AddRange(generatedLoans);
                context.SaveChanges();
            }
     
            }
    }
}
