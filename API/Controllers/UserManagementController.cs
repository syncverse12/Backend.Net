using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.UserManagement;
using SyncVerse.Application.Interfaces.Identity;

namespace SyncVerse.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UserManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR,Manager,ProjectManager")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userManagementService.GetAllUsersAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Message);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var result = await _userManagementService.GetUserByIdAsync(userId);
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.Message);
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDto dto)
        {
            var result = await _userManagementService.UpdateUserAsync(userId, dto);
            return result.IsSuccess ? Ok(new { message = "User updated successfully" }) : BadRequest(result.Message);
        }

        [HttpPut("{userId}/roles")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] List<string> newRoles)
        {
            var result = await _userManagementService.UpdateUserRolesAsync(userId, newRoles);
            return result.IsSuccess ? Ok(new { message = "Roles updated successfully" }) : BadRequest(result.Message);
        }

        [HttpPut("{userId}/lockout")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> ToggleUserLockout(string userId, [FromBody] bool lockUser)
        {
            var result = await _userManagementService.ToggleUserLockoutAsync(userId, lockUser);
            return result.IsSuccess ? Ok(new { message = $"User lockout status updated to: {lockUser}" }) : BadRequest(result.Message);
        }
    }
}