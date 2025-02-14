using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comp2139_Assignment1.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public required string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    
    [Required]
    public required decimal Price { get; set; }
    
    [Required]
    public required int Quantity { get; set; }
    
    public int Lowthreshhold { get; set; }
}