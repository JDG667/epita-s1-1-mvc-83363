using System.Threading.Tasks;
using Library.MVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

namespace Library.MVC
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
          
            var builder = WebApplication.CreateBuilder(args);

            // --- 1. CONFIGURATION DE SERILOG ---
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "FoodInspectionApp")
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog(); // Remplace le système de log par défaut

            // --- 2. SERVICES (Base de données & Identity) ---
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>() // Indispensable pour les rôles Admin/Inspector
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // --- 3. PIPELINE HTTP (Middlewares) ---
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // C'est ici que la "Friendly Page" est activée
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // --- 4. CONFIGURATION UNIQUE DE SERILOG ---
            // On ne met qu'UN SEUL UseSerilogRequestLogging avec les options d'enrichissement
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    // Capture le nom de l'utilisateur pour chaque log de requête
                    diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "Anonymous");
                    diagnosticContext.Set("Environment", app.Environment.EnvironmentName);
                };
            });

            app.UseAuthentication();
            app.UseAuthorization();

            // --- 5. ROUTES & SEED ---
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

        

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();



            // --- 6. INITIALISATION DE LA BASE (Seed) ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                // On appelle directement l'initialisation. 
                // Si ça plante, l'app s'arrêtera avec un message d'erreur clair.
                await DbInitializer.InitializeAsync(services);
            }

            try
            {
                Log.Information("Starting the app");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "App failed at the start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    } 
}
