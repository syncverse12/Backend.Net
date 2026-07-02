using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.Meetings;
using SyncVerse.Application.Interfaces.Meetings;
using SyncVerse.Application.Common.Results;

namespace SyncVerse.API.Controllers.Meetings
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;

        public MeetingsController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMeetingDto dto)
        {
            if (dto == null)
                return BadRequest("Meeting data cannot be empty.");

            var result = await _meetingService.CreateAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result); 
        }

        [HttpGet]
        public async Task<IActionResult> GetActive([FromQuery] string orgCode)
        {
            if (string.IsNullOrWhiteSpace(orgCode))
                return BadRequest("Organization code is required.");

            var meetings = await _meetingService.GetActiveMeetings(orgCode);
            return Ok(meetings);
        }

        [HttpDelete("{roomId}")]
        public async Task<IActionResult> Delete(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                return BadRequest("Room ID is required.");

            var deleted = await _meetingService.DeleteMeeting(roomId);
            if (!deleted)
                return NotFound("Meeting room not found.");

            return Ok(new { message = "Meeting deleted successfully." });
        }

        [HttpGet("{meetingId}/summary")]
        public async Task<IActionResult> GetMeetingSummary(string meetingId)
        {
            if (string.IsNullOrWhiteSpace(meetingId))
                return BadRequest("Meeting ID is required.");

            return Ok(new { message = "Feature ready to fetch saved summaries." });
        }
    }
}