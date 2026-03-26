using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace SyncVerse.Infrastructure.SeedConfiguration
{
    public static class DefaultAdminSeeder
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            string[] roleNames = { 
                "Admin", 
                "HR", 
                "WorkspaceAdmin",
                "Manager", 
                "Employee", 
                "ProjectManager", 
                "TeamLeader", 
                "TeamMember", 
                "Reviewer", 
                "QA", 
                "Observer" 
            };
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        Description = $"{roleName} role"
                    });
                }
            }

            await CreateOrUpdateUserAsync(userManager, "admin@syncverse.com", "Admin", "Admin@123!", Department.Engineering, SeniorityLevel.Lead);
            await CreateOrUpdateUserAsync(userManager, "hr@syncverse.com", "HR", "Hr@123!", Department.HR, SeniorityLevel.Senior);
            await CreateOrUpdateUserAsync(userManager, "pm@syncverse.com", "ProjectManager", "Pm@123!", Department.Engineering, SeniorityLevel.Senior);
            await CreateOrUpdateUserAsync(userManager, "manager@syncverse.com", "Manager", "Manager@123!", Department.Engineering, SeniorityLevel.Senior);
        }

        private static async Task CreateOrUpdateUserAsync(UserManager<User> userManager, string email, string role, string password, Department dept, SeniorityLevel level)
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
                    Department = dept,
                    SeniorityLevel = level,
                    EmailConfirmed = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(newUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, role);
                    Console.WriteLine($"✅ Successfully Created: {email}");
                }
                else
                {
                    Console.WriteLine($"❌ Error creating {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                user.FirstName = "Default";
                user.LastName = role;
                user.Department = dept;
                user.SeniorityLevel = level;
                user.EmailConfirmed = true;
                user.IsEmailVerified = true;

                var updateResult = await userManager.UpdateAsync(user);

                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, resetToken, password);

                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }

                Console.WriteLine($"✅ Successfully Updated: {email}");
            }
        }
    }
}