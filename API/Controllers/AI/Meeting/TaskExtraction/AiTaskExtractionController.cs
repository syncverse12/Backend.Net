using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.AI.Meeting.TaskExtraction;
using SyncVerse.Application.Interfaces.AI.Meeting.TaskExtraction;

namespace SyncVerse.API.Controllers.AI.Meeting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiTaskExtractionController : ControllerBase
    {
        private readonly IAiTaskExtractionService _aiTaskExtractionService;

        public AiTaskExtractionController(IAiTaskExtractionService aiTaskExtractionService)
        {
            _aiTaskExtractionService = aiTaskExtractionService;
        }

        [HttpPost("extract-tasks")]
        [AllowAnonymous] 
        public async Task<IActionResult> ExtractTasks([FromBody] AiTaskExtractionRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Transcript))
            {
                return BadRequest("Transcript cannot be empty.");
            }

            var result = await _aiTaskExtractionService.ExtractTasksAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
    }
}