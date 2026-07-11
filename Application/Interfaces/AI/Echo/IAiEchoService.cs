using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Echo;

namespace SyncVerse.Application.Interfaces.AI.Echo
{
    public interface IAiEchoService
    {
        Task<Result<EchoChatResponseDto>> TalkToEchoAsync(EchoChatRequestDto dto);
        System.Threading.Tasks.Task SaveProjectMemoryAutomatedAsync(EchoMemoryUploadDto memoryDto);
        Task<EchoTimelineResponseDto> GetProjectTimelineAsync(Guid projectId, int limit = 100, int offset = 0, string? memoryType = null, string? teamName = null);
        Task<EchoWeeklySummaryResponseDto> GetWeeklySummaryAsync(Guid projectId);
    }
}
