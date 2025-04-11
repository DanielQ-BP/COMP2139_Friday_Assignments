using Comp2139_Assignment1.Models;

namespace Comp2139_Assignment1.Areas.InventoryManagement.Models
{
    public class Orders
    {
        public int Id { get; set; }
    
        private DateTime _orderDate;
    
        public DateTime OrderDate
        {
            get => _orderDate;
            set => _orderDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }
    
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
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