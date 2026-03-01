using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Tasks.Employee;

namespace SyncVerse.Application.Interfaces.Tasks.TimeTracking
{
    public interface ITimeLogService
    {
        Task<Result<TimeLogResponseDto>> StartTimeLogAsync(string taskId, string userId, StartTimeLogDto dto);
        Task<Result<TimeLogResponseDto>> StopTimeLogAsync(string taskId, string userId, StopTimeLogDto dto);
        Task<Result<TimeLogResponseDto>> CreateManualTimeLogAsync(string taskId, string userId, CreateTimeLogDto dto);
        Task<Result<List<TimeLogResponseDto>>> GetTaskTimeLogsAsync(string taskId, string userId);
        Task<Result<List<TimeLogResponseDto>>> GetMyTimeLogsAsync(string userId, DateTime? fromDate, DateTime? toDate);
        Task<Result<int>> GetTotalTimeSpentAsync(string taskId, string userId);
        Task<Result<TaskTimeStatsDto>> GetActiveWorkingTimeAsync(string taskId, string userId);
    }
}
