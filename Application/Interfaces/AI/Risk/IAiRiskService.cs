using System;
using System.Threading.Tasks;

namespace SyncVerse.Application.Interfaces.AI.Risk
{
    public interface IAiRiskService
    {
        Task<object> AnalyzeProjectRisksAsync(Guid projectId);
        Task<object> GetProjectRiskHistoryAsync(Guid projectId, int limit = 20);
    }
}