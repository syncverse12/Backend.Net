using SyncVerse.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace SyncVerse.Infrastructure.SeedConfiguration
{
    public static class DefaultAdminSeeder
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            string[] roleNames = { "Admin", "Manager", "Employee" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        Description = $"{roleName} role for SyncVerse system"  
                    });
                }
            }

            await CreateUserAsync(userManager, "admin@domain.com", "Admin", "Admin123!");

            await CreateUserAsync(userManager, "manager@domain.com", "Manager", "Manager123!");

            await CreateUserAsync(userManager, "employee@domain.com", "Employee", "Employee123!");
        }

        private static async Task CreateUserAsync(UserManager<User> userManager, string email, string role, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var newUser = new User
                {
                    FirstName = "Default",
                    LastName = role, 
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, role);
                }
            }
        }
    }
}