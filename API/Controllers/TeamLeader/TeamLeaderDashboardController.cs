using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Dashboard;
using SyncVerse.Application.Interfaces.Dashboard;
using SyncVerse.Application.Interfaces.Identity;

namespace SyncVerse.API.Controllers.TeamLeader
{
    [ApiController]
    [Route("api/team-leader/dashboard")]
    [Authorize]
    public class TeamLeaderDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;

        public TeamLeaderDashboardController(
            IDashboardService dashboardService,
            ICurrentUserService currentUserService)
        {
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<TeamLeaderDashboardDto>>> GetDashboard()
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _dashboardService.GetTeamLeaderDashboardAsync(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
