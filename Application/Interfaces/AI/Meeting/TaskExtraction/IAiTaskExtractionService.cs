using global::SyncVerse.Application.Common.Results;
using global::SyncVerse.Application.DTOs.AI.Meeting.TaskExtraction;
namespace SyncVerse.Application.Interfaces.AI.Meeting.TaskExtraction
{
    public interface IAiTaskExtractionService
    {
        Task<Result<AiTaskExtractionResponseDto>> ExtractTasksAsync(AiTaskExtractionRequestDto dto);
    }
}
