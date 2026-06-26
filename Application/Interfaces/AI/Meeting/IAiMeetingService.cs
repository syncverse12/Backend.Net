using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Meeting;

namespace SyncVerse.Application.Interfaces.AI
{
    public interface IAiMeetingService
    {
        Task<Result<AiMeetingSummaryResponseDto>> GenerateSummaryAsync(AiMeetingSummaryRequestDto dto);
    }
}