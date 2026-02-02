using Graduation_Project.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Graduation_Project.Infrastructure.SeedConfiguration
{
    public static class DefaultAdminSeeder
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            
            var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
            if (!adminRoleExists)
            {
                await roleManager.CreateAsync(new Role
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "The Admin Role For The User"
                });
            }

            var adminUser = await userManager.FindByEmailAsync("admin@domain.com");
            if (adminUser == null)
            {
                var user = new User
                {
                    FirstName = "Default",
                    LastName = "Admin",
                    Email = "admin@domain.com",
                    UserName = "admin@domain.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
