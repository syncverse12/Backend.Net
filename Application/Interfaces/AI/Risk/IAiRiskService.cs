using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Risk;
using System;
using System.Threading.Tasks;

namespace SyncVerse.Application.Interfaces.AI.Risk
{
    public interface IAiRiskService
    {
        Task<Result<ProjectRiskResponseDto>> AnalyzeProjectRisksAsync(Guid projectId);
        Task<Result<ProjectRiskResponseDto>> UpdateLiveRisksAsync(LiveRiskUpdateRequestDto dto);
        Task<Result<bool>> SaveProjectRiskProfileAsync(Guid projectId, ProjectRiskProfileEnrichmentDto enrichmentData);
    }
}