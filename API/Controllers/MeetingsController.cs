using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.Meetings;
using SyncVerse.Application.Interfaces.Meetings;

[ApiController]
[Route("api/meetings")]
public class MeetingsController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public MeetingsController(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    // POST /api/meetings
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeetingDto dto)
    {
        var result = await _meetingService.CreateAsync(dto);
        return Ok(result.Data);
    }

    // GET /api/meetings?orgCode=...
    [HttpGet]
    public async Task<IActionResult> GetActive([FromQuery] string orgCode)
    {
        var meetings = await _meetingService.GetActiveMeetings(orgCode);
        return Ok(meetings);
    }

    // DELETE /api/meetings/{roomId}
    [HttpDelete("{roomId}")]
    public async Task<IActionResult> Delete(string roomId)
    {
        var deleted = await _meetingService.DeleteMeeting(roomId);
        if (!deleted) return NotFound();
        return Ok(new { message = "Meeting deleted successfully." });
    }
}
