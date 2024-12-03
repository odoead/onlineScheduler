using CompanyService.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using System.Reflection;

namespace CompanyService.DB
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<PersonalCompany> PersonalCompanies { get; set; }
        public DbSet<SharedCompany> SharedCompanies { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CompanyWorker> СompanyWorkers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<ScheduleInterval> ScheduleIntervals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.Entity<Company>().HasDiscriminator<int>("CompanyType")
                .HasValue<PersonalCompany>((int)CompanyType.PERSONAL)
                .HasValue<SharedCompany>((int)CompanyType.SHARED);

            // Define composite key for CompanyWorkers
            builder.Entity<CompanyWorker>()
                .HasKey(cw => new { cw.CompanyID, cw.WorkerId });
            builder.Entity<CompanyWorker>()
                .HasOne(cw => cw.Company)
                .WithMany(c => c.Workers)
                .HasForeignKey(cw => cw.CompanyID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<CompanyWorker>()
                .HasOne(cw => cw.Worker)
                .WithMany(u => u.CompanyWorkAssignments)
                .HasForeignKey(cw => cw.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductWorker>()
                .HasKey(cw => new { cw.ProductId, cw.WorkerId });
            builder.Entity<ProductWorker>()
                .HasOne(cw => cw.Product)
                .WithMany(c => c.AssignedWorkers)
                .HasForeignKey(cw => cw.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ProductWorker>()
                .HasOne(cw => cw.Worker)
                .WithMany(u => u.AssignedProducts)
                .HasForeignKey(cw => cw.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ScheduleInterval>()
           .Property(e => e.IntervalType)
           .HasConversion<int>();

            builder.Entity<ScheduleInterval>().Property(q => q.WeekDay).HasConversion<int>();
        }

    }

}
