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
    }
}
