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
    }
}
