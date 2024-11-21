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


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<Booking>()
            .Property(e => e.Status)
            .HasConversion<int>();



        }

    }


}
