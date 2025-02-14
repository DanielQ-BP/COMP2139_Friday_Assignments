using Microsoft.EntityFrameworkCore;
using Comp2139_Assignment1.Models;
namespace Comp2139_Assignment1.Data
{
    public class InventoryDBContext : DbContext
    {
        public InventoryDBContext(DbContextOptions<InventoryDBContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

       
    }
}