using BookingService.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BookingService.DB
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ScheduleInterval> ScheduleIntervals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.Entity<Booking>()
            .Property(e => e.Status)
            .HasConversion<int>();


            builder.Entity<ScheduleInterval>()
           .Property(e => e.WeekDay)
           .HasConversion<int>();
            /*  builder.Entity<Company>().HasDiscriminator<int>("CompanyType")
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
                  .OnDelete(DeleteBehavior.Cascade);*/
        }

    }


}
