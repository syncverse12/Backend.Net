using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.AI.Echo;
using SyncVerse.Application.Interfaces.AI.Echo;

namespace SyncVerse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiEchoController : ControllerBase
    {
        private readonly IAiEchoService _echoService;

        public AiEchoController(IAiEchoService echoService)
        {
            _echoService = echoService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ChatWithEcho([FromBody] EchoChatRequestDto dto)
        {
            var result = await _echoService.TalkToEchoAsync(dto);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        [HttpGet("project/{projectId}/timeline")]
        public async Task<IActionResult> GetProjectTimeline([FromRoute] Guid projectId,[FromQuery] int limit = 100,
           [FromQuery] int offset = 0,
           [FromQuery] string? memoryType = null,
           [FromQuery] string? teamName = null)
        {
            var timeline = await _echoService.GetProjectTimelineAsync(projectId, limit, offset, memoryType, teamName);
            return Ok(timeline);
        }

        [HttpGet("project/{projectId}/weekly-summary")]
        public async Task<IActionResult> GetWeeklySummary([FromRoute] Guid projectId)
        {
            var summary = await _echoService.GetWeeklySummaryAsync(projectId);
            return Ok(summary);
        }
    }
}