using Comp2139_Assignment1.Areas.InventoryManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace Comp2139_Assignment1.Data;

public class ContextSeed
{
    public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.SuperAdmin.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.Admin.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.Moderator.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Enum.Roles.Basic.ToString()));
    }

    public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var defaultUser = new ApplicationUser
        {
            UserName = "superadmin",
            Email = "superadmin@gmail.com",
            FullName = "SuperAdmin",

            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
        };

        if (userManager.Users.All(u => u.Id != defaultUser.Id))
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);

            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Password1!");

                await userManager.AddToRoleAsync(defaultUser, Enum.Roles.Basic.ToString());
                await userManager.AddToRoleAsync(defaultUser, Enum.Roles.Moderator.ToString());
                await userManager.AddToRoleAsync(defaultUser, Enum.Roles.Admin.ToString());
                await userManager.AddToRoleAsync(defaultUser, Enum.Roles.SuperAdmin.ToString());
            }
        }
    }
}