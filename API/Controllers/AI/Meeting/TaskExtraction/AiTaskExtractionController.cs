using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.AI.Meeting.TaskExtraction;
using SyncVerse.Application.Interfaces.AI.Meeting.TaskExtraction;
using SyncVerse.Application.Interfaces.Task.Manager;

namespace SyncVerse.API.Controllers.AI.Meeting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiTaskExtractionController : ControllerBase
    {
        private readonly IAiTaskExtractionService _aiTaskExtractionService;
        private readonly ITaskService _taskService;

        public AiTaskExtractionController(
            IAiTaskExtractionService aiTaskExtractionService,
            ITaskService taskService)
        {
            _aiTaskExtractionService = aiTaskExtractionService;
            _taskService = taskService;
        }

        [HttpPost("extract-tasks")]
        public async Task<IActionResult> ExtractTasks([FromBody] AiTaskExtractionRequestDto dto)
        {
            var result = await _aiTaskExtractionService.ExtractTasksAsync(dto);
            if (result.IsSuccess) return Ok(result.Data);
            return BadRequest(result.Message);
        }

        [HttpPost("save-confirmed-tasks")]
        public async Task<IActionResult> SaveConfirmedTasks(
            [FromBody] List<AiExtractedTaskDto> extractedTasks,
            [FromQuery] string projectId,
            [FromQuery] string milestoneId)
        {
            if (extractedTasks == null || !extractedTasks.Any())
            {
                return BadRequest("The extracted tasks list cannot be empty.");
            }

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var result = await _taskService.SaveExtractedTasksAsync(extractedTasks, projectId, milestoneId, currentUserId);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
    }
}