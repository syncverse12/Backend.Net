using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Dashboard;
using SyncVerse.Application.Interfaces.Dashboard;
using SyncVerse.Application.Interfaces.Identity;

namespace SyncVerse.API.Controllers.HR
{
    [ApiController]
    [Route("api/hr/dashboard")]
    [Authorize] // You can apply specific HR policy here if required, e.g., [Authorize(Roles = "HR")]
    public class HRDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;

        public HRDashboardController(
            IDashboardService dashboardService,
            ICurrentUserService currentUserService)
        {
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<HRDashboardDto>>> GetDashboard()
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _dashboardService.GetHRDashboardAsync(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
