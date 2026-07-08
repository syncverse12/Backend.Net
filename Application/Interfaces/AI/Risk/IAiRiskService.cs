using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Risk;

namespace SyncVerse.Application.Interfaces.AI.Risk
{
    public interface IAiRiskAssessmentService
    {
        Task<Result<ProjectRiskAssessmentResponseDto>> AnalyzeProjectRisksAsync(string projectId);
    }
}