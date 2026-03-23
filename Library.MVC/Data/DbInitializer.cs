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

            // --- ÉTAPE 0 : NETTOYAGE DES DONNÉES EXISTANTES ---
            context.FollowUps.RemoveRange(context.FollowUps);
            context.Inspections.RemoveRange(context.Inspections);
            context.Premises.RemoveRange(context.Premises);
            await context.SaveChangesAsync();

            // --- 1. ROLES (Ajout de Member) ---
            string[] roles = { "Admin", "Inspector", "Member" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // --- 1.5. USERS (Admin, Inspector, Member) ---
            var password = "Password123!";
            var users = new (string Email, string Role)[]
            {
                ("admin@safety.com", "Admin"),
                ("inspector@safety.com", "Inspector"),
                ("member@safety.com", "Member") // Voici ton utilisateur Member
            };

            foreach (var u in users)
            {
                if (await userManager.FindByEmailAsync(u.Email) == null)
                {
                    var newUser = new IdentityUser { UserName = u.Email, Email = u.Email, EmailConfirmed = true };
                    await userManager.CreateAsync(newUser, password);
                    await userManager.AddToRoleAsync(newUser, u.Role);
                }
            }

            // --- 2. SEED 12 PREMISES ---
            if (!context.Premises.Any())
            {
                var towns = new[] { "London", "Manchester", "Liverpool" };
                var ratings = new[] { "Low", "Medium", "High" };

                var premisesFaker = new Faker<Premises>()
                    .RuleFor(p => p.Name, f => f.Company.CompanyName().Replace(",", "") + " " + f.PickRandom("Kitchen", "Bistro", "Cafe", "Grill"))
                    .RuleFor(p => p.Address, f => f.Address.BuildingNumber() + " " + f.Address.StreetName())
                    .RuleFor(p => p.Town, f => f.PickRandom(towns))
                    .RuleFor(p => p.RiskRating, f => f.PickRandom(ratings));

                context.Premises.AddRange(premisesFaker.Generate(12));
                await context.SaveChangesAsync();
            }

            // --- 3. SEED 25 INSPECTIONS ---
            if (!context.Inspections.Any())
            {
                var premisesIds = context.Premises.Select(p => p.Id).ToList();

                var inspectionFaker = new Faker<Inspection>()
                    .RuleFor(i => i.InspectionDate, f => f.Date.Recent(60))
                    .RuleFor(i => i.Score, f => f.Random.Int(20, 100))
                    .RuleFor(i => i.Outcome, (f, i) => i.Score >= 70 ? "Pass" : "Fail")
                    .RuleFor(i => i.Notes, f => f.Lorem.Sentence())
                    .RuleFor(i => i.PremisesId, f => f.PickRandom(premisesIds));

                context.Inspections.AddRange(inspectionFaker.Generate(25));
                await context.SaveChangesAsync();
            }

            // --- 4. SEED 10 FOLLOW-UPS (Liés uniquement aux inspections Fail) ---
            if (!context.FollowUps.Any())
            {
                // On ne prend que les inspections qui ont échoué
                var failedIds = context.Inspections
                    .Where(i => i.Score < 70)
                    .Select(i => i.Id)
                    .ToList();

                if (failedIds.Any())
                {
                    int count = 0;
                    var followUpFaker = new Faker<FollowUp>()
                        .RuleFor(f => f.InspectionId, f => failedIds[count++])
                        .RuleFor(f => f.DueDate, (f, u) => count <= 4
                            ? f.Date.Past(1)  // Overdue
                            : f.Date.Soon(14)) // Future
                        .RuleFor(f => f.Status, (f, u) => count <= 4 ? "Open" : "Closed");

                    // On en génère 10 (ou le max possible si moins de 10 échecs)
                    context.FollowUps.AddRange(followUpFaker.Generate(Math.Min(10, failedIds.Count)));
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}