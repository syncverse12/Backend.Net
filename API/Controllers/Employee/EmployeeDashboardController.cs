using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Dashboard;
using SyncVerse.Application.Interfaces.Dashboard;
using SyncVerse.Application.Interfaces.Identity;

namespace SyncVerse.API.Controllers.Employee
{
    [ApiController]
    [Route("api/employee/dashboard")]
    [Authorize(Policy = "EmployeeOnly")]
    public class EmployeeDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUserService;

        public EmployeeDashboardController(
            IDashboardService dashboardService,
            ICurrentUserService currentUserService)
        {
            _dashboardService = dashboardService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<EmployeeDashboardDto>>> GetDashboard()
        {
            var userId = _currentUserService.UserId!;
            var result = await _dashboardService.GetEmployeeDashboardAsync(userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
