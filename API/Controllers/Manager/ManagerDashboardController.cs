using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Dashboard;
using SyncVerse.Application.Interfaces.Dashboard;
using SyncVerse.Application.Interfaces.Identity;

namespace SyncVerse.API.Controllers.Manager
{
    [ApiController]
    [Route("api/manager/dashboard")]
    [Authorize(Roles = "Manager")]
    public class ManagerDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;

        public ManagerDashboardController(
            IDashboardService dashboardService,
            ICurrentUserService currentUserService)
        {
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<ManagerDashboardDto>>> GetDashboard()
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _dashboardService.GetManagerDashboardAsync(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("tasks")]
        public async Task<ActionResult<Result<ManagerTaskDashboardDto>>> GetTaskDashboard()
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _dashboardService.GetManagerTaskDashboardAsync(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("projects/{projectId}/tasks")]
        public async Task<ActionResult<Result<TaskDashboardDto>>> GetProjectTaskDashboard(string projectId)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _dashboardService.GetProjectTaskDashboardAsync(projectId, userId);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
