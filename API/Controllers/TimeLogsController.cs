using Graduation_Project.Application.DTOs.Tasks.Employee;
using Graduation_Project.Application.Interfaces.Tasks.TimeTracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/{taskId}/timelogs")]
    public class TimeLogsController : ControllerBase
    {
        private readonly ITimeLogService _timeLogService;

        public TimeLogsController(ITimeLogService timeLogService)
        {
            _timeLogService = timeLogService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartTimeLog(string taskId, [FromBody] StartTimeLogDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeLogService.StartTimeLogAsync(taskId, userId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopTimeLog(string taskId, [FromBody] StopTimeLogDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeLogService.StopTimeLogAsync(taskId, userId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("manual")]
        public async Task<IActionResult> CreateManualTimeLog(string taskId, [FromBody] CreateTimeLogDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeLogService.CreateManualTimeLogAsync(taskId, userId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskTimeLogs(string taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeLogService.GetTaskTimeLogsAsync(taskId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("active-time")]
        public async Task<IActionResult> GetActiveWorkingTime(string taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeLogService.GetActiveWorkingTimeAsync(taskId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalTimeSpent(string taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeLogService.GetTotalTimeSpentAsync(taskId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    [Authorize]
    [ApiController]
    [Route("api/employee/timelogs")]
    public class EmployeeTimeLogsController : ControllerBase
    {
        private readonly ITimeLogService _timeLogService;

        public EmployeeTimeLogsController(ITimeLogService timeLogService)
        {
            _timeLogService = timeLogService;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyTimeLogs([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeLogService.GetMyTimeLogsAsync(userId, fromDate, toDate);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
