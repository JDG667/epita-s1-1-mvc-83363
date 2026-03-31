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
                // On peut spécifier la locale "en_GB" pour avoir des adresses qui sonnent britanniques
                // (Optionnel : var premisesFaker = new Faker<Premises>("en_GB")...)

                var towns = new[] { "London", "Manchester", "Liverpool" };
                var ratings = new[] { "Low", "Medium", "High" };

                var premisesFaker = new Faker<Premises>()
                    .RuleFor(p => p.Name, f => f.Company.CompanyName().Replace(",", "") + " " + f.PickRandom("Kitchen", "Bistro", "Cafe", "Grill"))

                    // CHANGEMENT ICI : StreetAddress génère "123 Main St" ou "45 Park Lane"
                    // C'est beaucoup plus propre que de concaténer BuildingNumber et StreetName manuellement.
                    .RuleFor(p => p.Address, f => f.Address.StreetAddress())

                    .RuleFor(p => p.Town, f => f.PickRandom(towns))
                    .RuleFor(p => p.RiskRating, f => f.PickRandom(ratings));

                context.Premises.AddRange(premisesFaker.Generate(12));
                await context.SaveChangesAsync();
            }

            // --- 3. SEED EXACTEMENT 25 INSPECTIONS ---
            if (!context.Inspections.Any())
            {
                var premisesIds = context.Premises.Select(p => p.Id).ToList();
                var inspectionFaker = new Faker<Inspection>()
                    .RuleFor(i => i.InspectionDate, f => f.Date.Recent(60))
                    // FORCE : On baisse le range du score pour garantir beaucoup d'échecs (Fail < 50)
                    .RuleFor(i => i.Score, f => f.Random.Int(10, 60))
                    .RuleFor(i => i.Outcome, (f, i) => i.Score >= 50 ? "Pass" : "Fail")
                    .RuleFor(i => i.Notes, f => f.Lorem.Sentence())
                    // On s'assure de piocher dans tous les établissements
                    .RuleFor(i => i.PremisesId, f => f.PickRandom(premisesIds));

                context.Inspections.AddRange(inspectionFaker.Generate(25));
                await context.SaveChangesAsync();
            }

            // --- 4. SEED EXACTEMENT 10 FOLLOW-UPS (5 Open / 5 Closed) ---
            if (!context.FollowUps.Any())
            {
                // On récupère les IDs des 10 premières inspections (qu'on a forcées en 'Fail' plus haut)
                // On trie par ID pour être sûr de l'ordre de traitement
                var idsForSeed = context.Inspections
                    .Where(i => i.Outcome == "Fail")
                    .OrderBy(i => i.Id)
                    .Select(i => i.Id)
                    .Take(10)
                    .ToList();

                // On initialise le compteur pour le Faker
                int internalCount = 0;

                var followUpFaker = new Faker<FollowUp>()
                    .CustomInstantiator(f =>
                    {
                        // Sécurité au cas où le count dépasserait la liste
                        if (internalCount >= idsForSeed.Count) return null;

                        var inspectionId = idsForSeed[internalCount];

                        // 0 à 4 = Open (Overdue) | 5 à 9 = Closed
                        var status = (internalCount < 5) ? "Open" : "Closed";
                        internalCount++;

                        return new FollowUp
                        {
                            InspectionId = inspectionId,
                            Status = status,
                            // Date d'échéance dans le passé pour simuler du retard sur les 'Open'
                            DueDate = f.Date.Past(1),
                            // Date de fermeture uniquement si le statut est Closed
                            ClosedDate = (status == "Closed") ? f.Date.Recent(3) : null
                        };
                    });

                // On génère uniquement si on a trouvé les IDs
                var itemsToSave = followUpFaker.Generate(idsForSeed.Count).Where(x => x != null).ToList();

                context.FollowUps.AddRange(itemsToSave);
                await context.SaveChangesAsync();
            }
        }
    }
}