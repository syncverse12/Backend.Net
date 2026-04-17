using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Profile;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Application.Interfaces.Profile
{
    public interface IProfileService
    {
        Task<Result<UserProfileDto>> GetProfileAsync(string userId);
        Task<Result<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<Result<bool>> ChangeEmailAsync(string userId, ChangeEmailDto dto);
        Task<Result<UserSettingsDto>> GetUserSettingsAsync(string userId);
        Task<Result<UserSettingsDto>> UpdateUserSettingsAsync(string userId, UserSettingsDto dto);
        Task<User?> GetUserWithWorkspaceAsync(string userId);
    }
}
