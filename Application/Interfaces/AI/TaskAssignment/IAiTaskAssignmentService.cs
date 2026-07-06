using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.TaskAssignment;
using System.Threading.Tasks;

namespace SyncVerse.Application.Interfaces.AI.TaskAssignment
{
    public interface IAiTaskAssignmentService
    {
        Task<Result<AiTaskAnalysisResponseDto>> AnalyzeTaskAsync(AiTaskAnalysisRequestDto requestDto);
        Task<Result<object>> AnalyzeTaskSyncAsync(AiTaskAnalysisRequestDto requestDto);
        Task<Result<object>> GetEmployeesAsync();
        Task<Result<object>> AddEmployeeAsync(AiAddEmployeeRequestDto requestDto);
        Task<Result<object>> UpdateEmployeeStatusAsync(AiUpdateEmployeeStatusRequestDto requestDto);
        Task<Result<object>> CheckRootAsync();
        Task<Result<object>> CheckHealthAsync();
    }
}
