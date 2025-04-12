using Comp2139_Assignment1.Areas.InventoryManagement.Models;
using Comp2139_Assignment1.Models;
using Microsoft.EntityFrameworkCore;

namespace Comp2139_Assignment1.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new InventoryDBContext(
                serviceProvider.GetRequiredService<DbContextOptions<InventoryDBContext>>()))
            {
                // Skip seeding if already seeded
                if (context.Categories.Any() || context.Products.Any() || context.Orders.Any())
                    return;

                // Seed Categories
                var electronics = new Category { Name = "Electronics" };
                var clothing = new Category { Name = "Clothing" };
                var food = new Category { Name = "Food" };

                context.Categories.AddRange(electronics, clothing, food);
                context.SaveChanges();

                // Seed Products
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Smartphone",
                        Description = "Latest model smartphone",
                        CategoryId = electronics.Id,
                        Price = 699.99m,
                        Quantity = 50,
                        Lowthreshhold = 10
                    },
                    new Product
                    {
                        Name = "Laptop",
                        Description = "High-performance laptop",
                        CategoryId = electronics.Id,
                        Price = 1299.99m,
                        Quantity = 30,
                        Lowthreshhold = 5
                    },
                    new Product
                    {
                        Name = "T-Shirt",
                        Description = "Cotton t-shirt",
                        CategoryId = clothing.Id,
                        Price = 19.99m,
                        Quantity = 100,
                        Lowthreshhold = 20
                    },
                    new Product
                    {
                        Name = "Jeans",
                        Description = "Denim jeans",
                        CategoryId = clothing.Id,
                        Price = 49.99m,
                        Quantity = 75,
                        Lowthreshhold = 15
                    },
                    new Product
                    {
                        Name = "Apple",
                        Description = "Fresh apples",
                        CategoryId = food.Id,
                        Price = 0.99m,
                        Quantity = 200,
                        Lowthreshhold = 50
                    },
                    new Product
                    {
                        Name = "Bread",
                        Description = "Whole grain bread",
                        CategoryId = food.Id,
                        Price = 2.99m,
                        Quantity = 150,
                        Lowthreshhold = 30
                    }
                };

                context.Products.AddRange(products);
                context.SaveChanges();

                // Refresh product IDs
                var productList = context.Products.ToList();

                // Seed Orders
                var orders = new List<Orders>
                {
                    new Orders
                    {
                        CustomerName = "John Doe",
                        ShippingAddress = "123 Main St, Toronto, ON",
                        OrderDate = DateTime.Now,
                        OrderItems = new List<OrderItem>
                        {
                            new OrderItem { ProductId = productList.First(p => p.Name == "Smartphone").Id, Quantity = 2 },
                            new OrderItem { ProductId = productList.First(p => p.Name == "T-Shirt").Id, Quantity = 5 }
                        }
                    },
                    new Orders
                    {
                        CustomerName = "Jane Smith",
                        ShippingAddress = "456 Elm St, Vancouver, BC",
                        OrderDate = DateTime.Now,
                        OrderItems = new List<OrderItem>
                        {
                            new OrderItem { ProductId = productList.First(p => p.Name == "Laptop").Id, Quantity = 1 },
                            new OrderItem { ProductId = productList.First(p => p.Name == "Jeans").Id, Quantity = 3 }
                        }
                    }
                };

                context.Orders.AddRange(orders);
                context.SaveChanges();
            }
        }
    }
}
