using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.UserManagement;

namespace SyncVerse.Application.Interfaces.Identity
{
    public interface IUserManagementService
    {
        Task<Result<List<UserListDto>>> GetAllUsersAsync();
        Task<Result<UserDetailsDto>> GetUserByIdAsync(string userId);
        Task<Result<bool>> UpdateUserRolesAsync(string userId, List<string> newRoles);
        Task<Result<bool>> UpdateUserAsync(string userId, UpdateUserDto dto);
        Task<Result<bool>> ToggleUserLockoutAsync(string userId, bool lockUser);
    }
}
