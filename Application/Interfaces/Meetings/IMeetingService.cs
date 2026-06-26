using SyncVerse.Application.DTOs.Meetings;
using SyncVerse.Application.Common.Results;

namespace SyncVerse.Application.Interfaces.Meetings
{
    public interface IMeetingService
    {
        Task<Result<MeetingResponseDto>> CreateAsync(CreateMeetingDto dto);
        Task<List<MeetingResponseDto>> GetActiveMeetings(string orgCode);
        Task<bool> DeleteMeeting(string roomId);

        Task<Result<bool>> SaveAiSummaryAsync(string meetingId, SyncVerse.Application.DTOs.AI.Meeting.AiMeetingSummaryResponseDto aiDto);
    }
}
