using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Risk;

namespace SyncVerse.Application.Interfaces.AI.Risk
{
    public interface IAiRiskService
    {
        Task<Result<ProjectRiskResponseDto>> AnalyzeProjectRisksAsync(ProjectRiskRequestDto dto);
        Task<Result<ProjectRiskResponseDto>> UpdateLiveRisksAsync(LiveRiskUpdateRequestDto dto);
    }
}
