using System;
using System.Collections.Generic;
using Comp2139_Assignment1.Models;

namespace Comp2139_Assignment1.Models
{
    public class Orders
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } // Relation to OrderItems
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } // Link to Product
        public int OrderId { get; set; }
        public Orders Orders { get; set; } // Link to Order
    }
}