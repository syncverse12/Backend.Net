using global::SyncVerse.Application.DTOs.AI.Meeting;
using global::SyncVerse.Application.Interfaces.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SyncVerse.API.Controllers.AI.Meeting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiMeetingController : ControllerBase
    {
        private readonly IAiMeetingService _aiMeetingService;

        public AiMeetingController(IAiMeetingService aiMeetingService)
        {
            _aiMeetingService = aiMeetingService;
        }

        [HttpPost("{meetingId}/transcribe-secure")]
        [AllowAnonymous]
        public async Task<IActionResult> TranscribeAudioSecure(string meetingId, IFormFile file)
        {
            var result = await _aiMeetingService.TranscribeAudioSecureAsync(file, meetingId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("{meetingId}/save-summary")]
        public async Task<IActionResult> ProcessAndSaveSummary(string meetingId, [FromBody] SecureProcessRequestDto dto)
        {
            var result = await _aiMeetingService.ProcessAndSaveSummaryAsync(meetingId, dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(new { message = result.Message });
        }

        [HttpPost("generate-summary")]
        public async Task<IActionResult> GenerateSummary([FromBody] AiMeetingSummaryRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Transcript))
            {
                return BadRequest("Transcript cannot be empty.");
            }

            var result = await _aiMeetingService.GenerateSummaryAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
    }
}