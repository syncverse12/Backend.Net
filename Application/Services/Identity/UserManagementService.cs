using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.UserManagement;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.Services.Identity
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<User> _userManager;

        public UserManagementService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<List<UserListDto>>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var dtos = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                dtos.Add(new UserListDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Department = user.Department.ToString(),
                    SeniorityLevel = user.SeniorityLevel.ToString(),
                    Roles = roles,
                    IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    CreatedAt = user.CreatedAt
                });
            }

            return Result<List<UserListDto>>.Success(dtos);
        }

        public async Task<Result<UserDetailsDto>> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<UserDetailsDto>.Failure("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var workspaceUsersCount = string.IsNullOrWhiteSpace(user.WorkspaceId)
                ? 0
                : await _userManager.Users.CountAsync(u => u.WorkspaceId == user.WorkspaceId);

            var teamSize = workspaceUsersCount <= 10
                ? 0
                : workspaceUsersCount <= 30
                    ? 1
                    : 2;

            var orgCode = string.IsNullOrWhiteSpace(user.WorkspaceId)
                ? "GENERAL"
                : user.WorkspaceId;

            var teamId = user.Department switch
            {
                Department.Engineering => "TECH",
                Department.ProductAndDesign => "UI",
                Department.QA => "QA",
                Department.HR => "HR",
                Department.Support => "SUPPORT",
                _ => "GENERAL"
            };

            var dto = new UserDetailsDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
                OrgCode = orgCode,
                TeamId = teamId,
                TeamSize = teamSize,
                Gender = "Unknown",
                Email = user.Email ?? string.Empty,
                Department = user.Department.ToString(),
                SeniorityLevel = user.SeniorityLevel.ToString(),
                Roles = roles,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                CreatedAt = user.CreatedAt,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Address = user.Address,
                Skills = user.Skills,
                IsEmailVerified = user.IsEmailVerified
            };

            return Result<UserDetailsDto>.Success(dto);
        }

        public async Task<Result<bool>> UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found.");
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Department = dto.Department;
            user.SeniorityLevel = dto.SeniorityLevel;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure($"Failed to update user: {errors}");
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> UpdateUserRolesAsync(string userId, List<string> newRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove current roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return Result<bool>.Failure("Failed to remove existing roles.");
            }

            // Add new roles
            var addResult = await _userManager.AddToRolesAsync(user, newRoles);
            if (!addResult.Succeeded)
            {
                return Result<bool>.Failure("Failed to add new roles.");
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ToggleUserLockoutAsync(string userId, bool lockUser)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found.");
            }

            if (lockUser)
            {
                // Lock out for basically forever (e.g. 100 years)
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }
            else
            {
                user.LockoutEnd = null;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result<bool>.Failure("Failed to update user lockout status.");
            }

            return Result<bool>.Success(true);
        }
    }
}
