using System.ComponentModel.DataAnnotations;
using Comp2139_Assignment1.Models;

namespace Comp2139_Assignment1.Areas.InventoryManagement.Models;

public class Category
{
    [Key] public int Id { get; set; }

    [Required]
    public required string Name { get; set; } = string.Empty;

    public List<Product> Products { get; set; } = new();
}