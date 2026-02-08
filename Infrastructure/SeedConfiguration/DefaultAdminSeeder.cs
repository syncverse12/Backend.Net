using Graduation_Project.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Graduation_Project.Infrastructure.SeedConfiguration
{
    public static class DefaultAdminSeeder
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            // 1️⃣ إنشاء الأدوار (Roles) اللي الـ Policies بتعتمد عليها
            // "Admin" للتحكم الكامل، "Manager" لـ ManagerOnly، "Employee" لـ EmployeeOnly
            string[] roleNames = { "Admin", "Manager", "Employee" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        Description = $"{roleName} role for Synverse system"
                    });
                }
            }

            // 2️⃣ إنشاء مستخدم Default Admin
            await CreateUserAsync(userManager, "admin@domain.com", "Admin", "Admin123!");

            // 3️⃣ إنشاء مستخدم Default Manager (عشان يحل مشكلة الـ 403 في الـ Workspaces)
            // ده اللي الـ ManagerAuthorizationHandler هيوافق عليه
            await CreateUserAsync(userManager, "manager@domain.com", "Manager", "Manager123!");

            // 4️⃣ إنشاء مستخدم Default Employee (عشان الـ EmployeeOnly والـ TaskOwner)
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
                    LastName = role, // بنسميه بإسم الدور عشان نميزه في الـ Swagger
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