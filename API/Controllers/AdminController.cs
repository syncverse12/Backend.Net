using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Dashboard;
using SyncVerse.Application.Interfaces.Dashboard;

namespace SyncVerse.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public AdminController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("dashboard")]
        public async Task<ActionResult<Result<AdminDashboardDto>>> Dashboard()
        {
            var result = await _dashboardService.GetAdminDashboardAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
