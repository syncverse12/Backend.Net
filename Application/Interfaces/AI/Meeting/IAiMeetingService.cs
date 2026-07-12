using Microsoft.AspNetCore.Http;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Meeting;

namespace SyncVerse.Application.Interfaces.AI
{
    public interface IAiMeetingService
    {
        Task<Result<TranscriptionSecureResponseDto>> TranscribeAudioSecureAsync(IFormFile audioFile, string meetingId);
        Task<Result<bool>> SaveTranscriptToCacheAsync(string meetingId, TranscriptionSecureResponseDto dto);
        Task<Result<TranscriptionSecureResponseDto>> GetTranscriptFromCacheAsync(string meetingId);
        Task<Result<bool>> ProcessAndSaveSummaryAsync(string meetingId, SecureProcessRequestDto dto);

        Task<Result<bool>> ProcessAndSaveTasksAsync(string meetingId, SecureProcessRequestDto dto);

        Task<Result<AiMeetingSummaryResponseDto>> GenerateSummaryAsync(AiMeetingSummaryRequestDto dto);
    }
}