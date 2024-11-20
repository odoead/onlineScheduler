using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;
using System.Reflection;

namespace NotificationService.DB
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Data> NotifcationData { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<Notification>()
               .Property(e => e.Service)
               .HasConversion<int>();
        }
    }

}
