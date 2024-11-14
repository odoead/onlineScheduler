using CompanyService.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CompanyService.DB
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<PersonalCompany> PersonalCompanies { get; set; }
        public DbSet<SharedCompany> SharedCompanies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CompanyWorkers> СompanyWorkers { get; set; }
        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.Entity<Company>().HasDiscriminator<int>("CompanyType")
                .HasValue<PersonalCompany>(1)
                .HasValue<SharedCompany>(2);


            base.OnModelCreating(builder);

            // Define composite key for CompanyWorkers
            builder.Entity<CompanyWorkers>()
                .HasKey(cw => new { cw.CompanyID, cw.WorkerId });
            builder.Entity<CompanyWorkers>()
                .HasOne(cw => cw.Company)
                .WithMany(c => c.Workers)
                .HasForeignKey(cw => cw.CompanyID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CompanyWorkers>()
                .HasOne(cw => cw.Worker)
                .WithMany(u => u.CompanyWorkers)
                .HasForeignKey(cw => cw.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }


}
