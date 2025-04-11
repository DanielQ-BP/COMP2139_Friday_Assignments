using Comp2139_Assignment1.Models;

namespace Comp2139_Assignment1.Areas.InventoryManagement.Models;

public class InventoryOverviewViewModel
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public List<Product> LowStockProducts { get; set; } = new();
}