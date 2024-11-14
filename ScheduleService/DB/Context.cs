using Microsoft.EntityFrameworkCore;
using ScheduleService.Entities;
using System.Reflection;

namespace ScheduleService.DB
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ScheduleInterval> ScheduleIntervals { get; set; }
        public DbSet<Employee> Employees { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        }

    }


}
