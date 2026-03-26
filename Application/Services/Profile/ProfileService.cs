using Microsoft.AspNetCore.Identity;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Profile;
using SyncVerse.Application.Interfaces.Profile;
using SyncVerse.Application.Interfaces.Storage;
using SyncVerse.Domain.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;
using SyncVerse.Infrastructure.Data;

namespace SyncVerse.Application.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly IFileStorageService _fileStorageService;
        private readonly DatabaseDbContext _context;

        public ProfileService(UserManager<User> userManager, IFileStorageService fileStorageService, DatabaseDbContext context)
        {
            _userManager = userManager;
            _fileStorageService = fileStorageService;
            _context = context;
        }

        public async Task<Result<UserProfileDto>> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result<UserProfileDto>.Failure("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            var profile = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Address = user.Address,
                Skills = user.Skills ?? new List<string>(),
                Department = user.Department, 
                SeniorityLevel = user.SeniorityLevel, 
                Roles = roles.ToList(),
                JoinedDate = user.CreatedAt
            };

            return Result<UserProfileDto>.Success(profile);
        }

        public async Task<Result<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result<UserProfileDto>.Failure("User not found");

            // updating basic information if provided
            if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Address)) user.Address = dto.Address;
            
            // updating skills
            if (dto.Skills != null) 
            {
                user.Skills = dto.Skills;
            }

            // if the user uploaded a new profile picture
            if (dto.ProfilePicture != null)
            {
                var fileExtension = Path.GetExtension(dto.ProfilePicture.FileName);
                var fileName = $"user_{user.Id}_{Guid.NewGuid()}{fileExtension}";
                using var stream = dto.ProfilePicture.OpenReadStream();
                
                var filePath = await _fileStorageService.UploadFileAsync(stream, fileName, "profile-pictures");
                user.ProfilePictureUrl = filePath;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result<UserProfileDto>.Failure("Failed to update profile");

            return await GetProfileAsync(userId); // returning the profile after successful update
        }

        public async Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result<bool>.Failure("User not found");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure($"Failed to change password: {errors}");
            }

            return Result<bool>.Success(true, "Password changed successfully.");
        }

        public async Task<Result<bool>> ChangeEmailAsync(string userId, ChangeEmailDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result<bool>.Failure("User not found");

            if (user.Email == dto.NewEmail)
                return Result<bool>.Failure("The new email is identical to the current one.");

            var existingUser = await _userManager.FindByEmailAsync(dto.NewEmail);
            if (existingUser != null)
                return Result<bool>.Failure("This email is already in use by another account.");

            var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail);
            var result = await _userManager.ChangeEmailAsync(user, dto.NewEmail, emailToken);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure($"Failed to change email: {errors}");
            }

            user.UserName = dto.NewEmail;
            user.IsEmailVerified = false; 
            user.EmailConfirmed = false;
            
            await _userManager.UpdateAsync(user);

            return Result<bool>.Success(true, "Email changed successfully. Please verify your new email.");
        }

        public async Task<Result<UserSettingsDto>> GetUserSettingsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result<UserSettingsDto>.Failure("User not found");

            var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

            // If settings don't exist yet, we can return defaults
            if (settings == null)
            {
                settings = new UserSettings { UserId = userId };
                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            var dto = new UserSettingsDto
            {
                Theme = settings.Theme,
                Language = settings.Language,
                TimeZoneId = settings.TimeZoneId,
                EnableEmailNotifications = settings.EnableEmailNotifications,
                EnableInAppNotifications = settings.EnableInAppNotifications,
                NotifyOnTaskAssignment = settings.NotifyOnTaskAssignment,
                TaskReminderAdvanceHours = settings.TaskReminderAdvanceHours,
                AvailabilityStatus = settings.AvailabilityStatus,
                StatusMessage = settings.StatusMessage,
                ShowEmailToTeam = settings.ShowEmailToTeam
            };

            return Result<UserSettingsDto>.Success(dto);
        }

        public async Task<Result<UserSettingsDto>> UpdateUserSettingsAsync(string userId, UserSettingsDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result<UserSettingsDto>.Failure("User not found");

            var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new UserSettings { UserId = userId };
                _context.UserSettings.Add(settings);
            }

            // Map updated settings
            settings.Theme = dto.Theme;
            settings.Language = dto.Language;
            settings.TimeZoneId = dto.TimeZoneId;
            settings.EnableEmailNotifications = dto.EnableEmailNotifications;
            settings.EnableInAppNotifications = dto.EnableInAppNotifications;
            settings.NotifyOnTaskAssignment = dto.NotifyOnTaskAssignment;
            settings.TaskReminderAdvanceHours = dto.TaskReminderAdvanceHours;
            settings.AvailabilityStatus = dto.AvailabilityStatus;
            settings.StatusMessage = dto.StatusMessage;
            settings.ShowEmailToTeam = dto.ShowEmailToTeam;

            await _context.SaveChangesAsync();

            return Result<UserSettingsDto>.Success(dto, "Settings updated successfully.");
        }
    }
}
