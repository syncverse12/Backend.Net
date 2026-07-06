using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI;

namespace SyncVerse.Application.Interfaces.AI
{
    public interface IAttritionPredictionService
    {
        Task<Result<AttritionPredictionResponseDto>> PredictAttritionAsync(string employeeId);
    }
}