using System.ComponentModel.DataAnnotations;

namespace Comp2139_Assignment1.Models;

public class Category
{
    [Key] public int Id { get; set; }

    [Required]
    public required string Name { get; set; } = string.Empty;

    public List<Product> Products { get; set; } = new();
}