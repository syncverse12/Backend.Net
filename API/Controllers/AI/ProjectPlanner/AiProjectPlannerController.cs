using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.AI.ProjectPlanner;
using SyncVerse.Application.Interfaces.AI.ProjectPlanner;
using System.Threading.Tasks;

namespace SyncVerse.API.Controllers.AI.ProjectPlanner
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AiProjectPlannerController : ControllerBase
    {
        private readonly IAiProjectPlannerService _aiProjectPlannerService;

        public AiProjectPlannerController(IAiProjectPlannerService aiProjectPlannerService)
        {
            _aiProjectPlannerService = aiProjectPlannerService;
        }

        [HttpPost("plan")]
        public async Task<IActionResult> CreatePlan([FromBody] AiProjectPlanRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _aiProjectPlannerService.CreateProjectPlanAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return StatusCode(201, result);
        }

        [HttpGet("plan/{projectId}")]
        public async Task<IActionResult> GetPlan(string projectId)
        {
            var result = await _aiProjectPlannerService.GetProjectPlanAsync(projectId);

            if (!result.IsSuccess)
            {
                return NotFound(result.Message); // Or BadRequest depending on how you want to handle it
            }

            return Ok(result);
        }

        [HttpDelete("plan/{projectId}")]
        public async Task<IActionResult> DeletePlan(string projectId)
        {
            var result = await _aiProjectPlannerService.DeleteProjectPlanAsync(projectId);

            if (!result.IsSuccess)
            {
                return NotFound(result.Message);
            }

            return NoContent();
        }

        [HttpGet("plan/{projectId}/summary")]
        public async Task<IActionResult> GetPlanSummary(string projectId)
        {
            var result = await _aiProjectPlannerService.GetProjectPlanSummaryAsync(projectId);

            if (!result.IsSuccess)
            {
                return NotFound(result.Message);
            }

            return Ok(result);
        }

        [HttpPost("plan/{projectId}/replan")]
        public async Task<IActionResult> Replan(string projectId, [FromBody] AiReplanRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _aiProjectPlannerService.ReplanProjectAsync(projectId, dto);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result.Message);
                }
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetAllPlans()
        {
            var result = await _aiProjectPlannerService.GetAllProjectPlansAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckHealth()
        {
            var result = await _aiProjectPlannerService.CheckHealthAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
    }
}
