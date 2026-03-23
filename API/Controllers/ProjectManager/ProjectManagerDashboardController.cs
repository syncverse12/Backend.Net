using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Dashboard;
using SyncVerse.Application.Interfaces.Dashboard;
using SyncVerse.Application.Interfaces.Identity;

namespace SyncVerse.API.Controllers.ProjectManager
{
    [ApiController]
    [Route("api/project-manager/dashboard")]
    [Authorize]
    public class ProjectManagerDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;

        public ProjectManagerDashboardController(
            IDashboardService dashboardService,
            ICurrentUserService currentUserService)
        {
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<ProjectManagerDashboardDto>>> GetDashboard()
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _dashboardService.GetProjectManagerDashboardAsync(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
