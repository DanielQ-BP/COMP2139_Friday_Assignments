using Comp2139_Assignment1.Areas.InventoryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Comp2139_Assignment1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Comp2139_Assignment1.Data
{
    public class InventoryDBContext : IdentityDbContext<ApplicationUser>
    {
        public InventoryDBContext(DbContextOptions<InventoryDBContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Orders>()
                .Property(o => o.OrderDate)
                .HasConversion(
                    v => v.ToUniversalTime(), 
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                );
        }

       
    }
    
    
}