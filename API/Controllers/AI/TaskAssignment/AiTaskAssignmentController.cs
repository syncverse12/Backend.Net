using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.AI.TaskAssignment;
using SyncVerse.Application.Interfaces.AI.TaskAssignment;
using System.Threading.Tasks;

namespace SyncVerse.API.Controllers.AI.TaskAssignment
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AiTaskAssignmentController : ControllerBase
    {
        private readonly IAiTaskAssignmentService _aiTaskAssignmentService;

        public AiTaskAssignmentController(IAiTaskAssignmentService aiTaskAssignmentService)
        {
            _aiTaskAssignmentService = aiTaskAssignmentService;
        }

        [HttpPost("analyze-task")]
        public async Task<IActionResult> AnalyzeTask([FromBody] AiTaskAnalysisRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _aiTaskAssignmentService.AnalyzeTaskAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
        [HttpPost("analyze-task/sync")]
        public async Task<IActionResult> AnalyzeTaskSync([FromBody] AiTaskAnalysisRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _aiTaskAssignmentService.AnalyzeTaskSyncAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var result = await _aiTaskAssignmentService.GetEmployeesAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpPost("add-employee")]
        public async Task<IActionResult> AddEmployee([FromBody] AiAddProjectEmployeesRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _aiTaskAssignmentService.AddEmployeeAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return StatusCode(201, result);
        }

        [HttpPost("update-employee-status")]
        public async Task<IActionResult> UpdateEmployeeStatus([FromBody] AiUpdateEmployeeStatusFrontendRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _aiTaskAssignmentService.UpdateEmployeeStatusAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpGet("root")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckRoot()
        {
            var result = await _aiTaskAssignmentService.CheckRootAsync();
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckHealth()
        {
            var result = await _aiTaskAssignmentService.CheckHealthAsync();
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpGet("calculate-availability/{userId}")]
        public async Task<IActionResult> CalculateAvailability(string userId)
        {
            var result = await _aiTaskAssignmentService.CalculateAvailabilityAsync(userId);
            if (!result.IsSuccess) return BadRequest(result.Message);
            
            return Ok(new 
            {
                UserId = userId,
                ActiveTasks = result.Data.ActiveTasks,
                AvailabilityScore = result.Data.AvailabilityScore
            });
        }
    }
}
