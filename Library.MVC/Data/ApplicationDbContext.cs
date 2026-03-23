using Library.MVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //add the databases for each models
        //needed for CRUD operations with EF Core

        public DbSet<Premises> Premises { get; set; }
        public DbSet<FollowUp> FollowUps { get; set; }
        public DbSet<Inspection> Inspections { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Inspection>()
                .HasOne(l => l.Premises)
                .WithMany()
                .HasForeignKey(l => l.PremisesId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<FollowUp>()
                .HasOne(l => l.Inspection)
                .WithMany()
                .HasForeignKey(l => l.InspectionId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
