using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.TaskAssignment;
using System.Threading.Tasks;

namespace SyncVerse.Application.Interfaces.AI.TaskAssignment
{
    public interface IAiTaskAssignmentService
    {
        Task<Result<AiTaskAnalysisResponseDto>> AnalyzeTaskAsync(AiTaskAnalysisRequestDto requestDto);
        Task<Result<object>> AnalyzeTaskSyncAsync(AiTaskAnalysisRequestDto requestDto);
    }
}
