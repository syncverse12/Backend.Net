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

            await CreateOrUpdateUserAsync(userManager, "admin@syncverse.com", "Admin", "Admin@123!", Department.Engineering, SeniorityLevel.Lead, "SyncVerse", "Admin");
            await CreateOrUpdateUserAsync(userManager, "omnia.elsaka92@gmail.com", "Admin", "Omnia@123!", Department.Engineering, SeniorityLevel.Lead, "Omnia", "Elsaka");
            await CreateOrUpdateUserAsync(userManager, "marwaabuelkheir12@gmail.com", "Admin", "Marwa@123!", Department.Engineering, SeniorityLevel.Lead, "Marwa", "Abuelkheir");
            await CreateOrUpdateUserAsync(userManager, "nade62204@gmail.com", "Admin", "Nada@123!", Department.Engineering, SeniorityLevel.Lead, "Nada", "Eslam");
            await CreateOrUpdateUserAsync(userManager, "hr@syncverse.com", "HR", "Hr@123!", Department.HR, SeniorityLevel.Senior, "HR", "SyncVerse");
            await CreateOrUpdateUserAsync(userManager, "pm@syncverse.com", "ProjectManager", "Pm@123!", Department.Engineering, SeniorityLevel.Senior, "Project", "Manager");
            await CreateOrUpdateUserAsync(userManager, "manager@syncverse.com", "Manager", "Manager@123!", Department.Engineering, SeniorityLevel.Senior, "General", "Manager");
            await CreateOrUpdateUserAsync(userManager, "mariamadham07@gmail.com", "Admin", "Mariam@123!", Department.Engineering, SeniorityLevel.Lead, "Mariam", "Adham");
            await CreateOrUpdateUserAsync(userManager, "maryamahmedb17@gmail.com", "Admin", "Maryam@123!", Department.Engineering, SeniorityLevel.Lead, "Maryam", "Ahmed");
        }

        private static async Task CreateOrUpdateUserAsync(UserManager<User> userManager, string email, string role, string password, Department dept, SeniorityLevel level, string firstName, string lastName)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var newUser = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
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
                user.FirstName = firstName;
                user.LastName = lastName;
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