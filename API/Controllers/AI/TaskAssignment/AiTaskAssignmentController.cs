using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.AI.TaskAssignment;
using SyncVerse.Application.Interfaces.AI.TaskAssignment;
using System.Threading.Tasks;

namespace SyncVerse.API.Controllers.AI.TaskAssignment
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
    }
}
