using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.ProjectPlanner;
using System.Threading.Tasks;

namespace SyncVerse.Application.Interfaces.AI.ProjectPlanner
{
    public interface IAiProjectPlannerService
    {
        Task<Result<object>> CreateProjectPlanAsync(AiProjectPlanRequestDto requestDto);
    }
}
