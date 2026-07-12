using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.ProjectPlanner;
using System.Threading.Tasks;

namespace SyncVerse.Application.Interfaces.AI.ProjectPlanner
{
    public interface IAiProjectPlannerService
    {
        Task<Result<object>> CreateProjectPlanAsync(AiProjectPlanRequestDto requestDto);
        Task<Result<object>> GetProjectPlanAsync(string projectId);
        Task<Result<bool>> DeleteProjectPlanAsync(string projectId);
        Task<Result<object>> GetProjectPlanSummaryAsync(string projectId);
        Task<Result<object>> ReplanProjectAsync(string projectId, AiReplanRequestDto requestDto);
        Task<Result<object>> GetAllProjectPlansAsync();
        Task<Result<object>> CheckHealthAsync();
        Task<Result<object>> GenerateScheduleForProjectAsync(string projectId);
    }
}
