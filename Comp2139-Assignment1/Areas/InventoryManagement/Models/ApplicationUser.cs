using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Comp2139_Assignment1.Areas.InventoryManagement.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    public required string FullName { get; set; }
}