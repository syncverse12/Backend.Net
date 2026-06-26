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

