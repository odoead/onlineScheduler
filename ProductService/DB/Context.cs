using Microsoft.EntityFrameworkCore;
using ProductService.Entities;
using System.Reflection;

namespace ProductService.DB
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<ProductWorker> ProductWorkers { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<ProductWorker>()
               .HasKey(cw => new { cw.ProductId, cw.WorkerId });
            builder.Entity<ProductWorker>()
                .HasOne(cw => cw.Product)
                .WithMany(c => c.Workers)
                .HasForeignKey(cw => cw.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductWorker>()
                .HasOne(cw => cw.Worker)
                .WithMany(u => u.Products)
                .HasForeignKey(cw => cw.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }


}
