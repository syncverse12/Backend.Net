using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Tasks.Employee;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Tasks.TimeTracking;
using SyncVerse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SyncVerse.Application.Services.Tasks.TimeTracking
{
    public class TimeLogService : ITimeLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TimeLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TimeLogResponseDto>> StartTimeLogAsync(string taskId, string userId, StartTimeLogDto dto)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<TimeLogResponseDto>.Failure("Task not found");

            if (task.AssignedToUserId != userId)
                return Result<TimeLogResponseDto>.Failure("You are not assigned to this task");

            var activeTimeLog = await _unitOfWork.Repository<TimeLog>()
                .Query()
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.UserId == userId && t.EndTime == null && !t.IsDeleted);

            if (activeTimeLog != null)
                return Result<TimeLogResponseDto>.Failure("You already have an active time log for this task. Please stop it first");

            var timeLog = new TimeLog
            {
                TaskId = taskId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                IsManual = false,
                Notes = dto.Notes
            };

            await _unitOfWork.Repository<TimeLog>().AddAsync(timeLog);
            await _unitOfWork.SaveChangesAsync();

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);

            var response = new TimeLogResponseDto
            {
                TimeLogId = timeLog.Id,
                TaskId = timeLog.TaskId,
                TaskTitle = task.Title,
                UserId = timeLog.UserId,
                UserName = user?.UserName ?? "Unknown",
                StartTime = timeLog.StartTime,
                EndTime = timeLog.EndTime,
                DurationInMinutes = timeLog.DurationInMinutes,
                IsManual = timeLog.IsManual,
                Notes = timeLog.Notes
            };

            return Result<TimeLogResponseDto>.Success(response, "Time tracking started successfully");
        }

        public async Task<Result<TimeLogResponseDto>> StopTimeLogAsync(string taskId, string userId, StopTimeLogDto dto)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<TimeLogResponseDto>.Failure("Task not found");

            var activeTimeLog = await _unitOfWork.Repository<TimeLog>()
                .Query()
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.UserId == userId && t.EndTime == null && !t.IsDeleted);

            if (activeTimeLog == null)
                return Result<TimeLogResponseDto>.Failure("No active time log found for this task");

            activeTimeLog.EndTime = DateTime.UtcNow;
            activeTimeLog.DurationInMinutes = (int)(activeTimeLog.EndTime.Value - activeTimeLog.StartTime).TotalMinutes;
            
            if (!string.IsNullOrWhiteSpace(dto.Notes))
                activeTimeLog.Notes = dto.Notes;

            _unitOfWork.Repository<TimeLog>().Update(activeTimeLog);
            await _unitOfWork.SaveChangesAsync();

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);

            var response = new TimeLogResponseDto
            {
                TimeLogId = activeTimeLog.Id,
                TaskId = activeTimeLog.TaskId,
                TaskTitle = task.Title,
                UserId = activeTimeLog.UserId,
                UserName = user?.UserName ?? "Unknown",
                StartTime = activeTimeLog.StartTime,
                EndTime = activeTimeLog.EndTime,
                DurationInMinutes = activeTimeLog.DurationInMinutes,
                IsManual = activeTimeLog.IsManual,
                Notes = activeTimeLog.Notes
            };

            return Result<TimeLogResponseDto>.Success(response, "Time tracking stopped successfully");
        }

        public async Task<Result<TimeLogResponseDto>> CreateManualTimeLogAsync(string taskId, string userId, CreateTimeLogDto dto)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<TimeLogResponseDto>.Failure("Task not found");

            if (task.AssignedToUserId != userId)
                return Result<TimeLogResponseDto>.Failure("You are not assigned to this task");

            if (dto.StartTime >= dto.EndTime)
                return Result<TimeLogResponseDto>.Failure("Start time must be before end time");

            var durationInMinutes = (int)(dto.EndTime - dto.StartTime).TotalMinutes;

            var timeLog = new TimeLog
            {
                TaskId = taskId,
                UserId = userId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                DurationInMinutes = durationInMinutes,
                IsManual = true,
                Notes = dto.Notes
            };

            await _unitOfWork.Repository<TimeLog>().AddAsync(timeLog);
            await _unitOfWork.SaveChangesAsync();

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);

            var response = new TimeLogResponseDto
            {
                TimeLogId = timeLog.Id,
                TaskId = timeLog.TaskId,
                TaskTitle = task.Title,
                UserId = timeLog.UserId,
                UserName = user?.UserName ?? "Unknown",
                StartTime = timeLog.StartTime,
                EndTime = timeLog.EndTime,
                DurationInMinutes = timeLog.DurationInMinutes,
                IsManual = timeLog.IsManual,
                Notes = timeLog.Notes
            };

            return Result<TimeLogResponseDto>.Success(response, "Manual time log created successfully");
        }

        public async Task<Result<List<TimeLogResponseDto>>> GetTaskTimeLogsAsync(string taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<List<TimeLogResponseDto>>.Failure("Task not found");

            if (task.AssignedToUserId != userId && task.CreatedByUserId != userId)
                return Result<List<TimeLogResponseDto>>.Failure("You don't have access to view time logs for this task");

            var timeLogs = await _unitOfWork.Repository<TimeLog>()
                .Query()
                .Include(t => t.User)
                .Include(t => t.Task)
                .Where(t => t.TaskId == taskId && !t.IsDeleted)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();

            var response = timeLogs.Select(t => new TimeLogResponseDto
            {
                TimeLogId = t.Id,
                TaskId = t.TaskId,
                TaskTitle = t.Task?.Title ?? "Unknown",
                UserId = t.UserId,
                UserName = t.User?.UserName ?? "Unknown",
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                DurationInMinutes = t.DurationInMinutes,
                IsManual = t.IsManual,
                Notes = t.Notes
            }).ToList();

            return Result<List<TimeLogResponseDto>>.Success(response);
        }

        public async Task<Result<List<TimeLogResponseDto>>> GetMyTimeLogsAsync(string userId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _unitOfWork.Repository<TimeLog>()
                .Query()
                .Include(t => t.User)
                .Include(t => t.Task)
                .Where(t => t.UserId == userId && !t.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(t => t.StartTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.StartTime <= toDate.Value);

            var timeLogs = await query
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();

            var response = timeLogs.Select(t => new TimeLogResponseDto
            {
                TimeLogId = t.Id,
                TaskId = t.TaskId,
                TaskTitle = t.Task?.Title ?? "Unknown",
                UserId = t.UserId,
                UserName = t.User?.UserName ?? "Unknown",
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                DurationInMinutes = t.DurationInMinutes,
                IsManual = t.IsManual,
                Notes = t.Notes
            }).ToList();

            return Result<List<TimeLogResponseDto>>.Success(response);
        }

        public async Task<Result<int>> GetTotalTimeSpentAsync(string taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<int>.Failure("Task not found");

            if (task.AssignedToUserId != userId && task.CreatedByUserId != userId)
                return Result<int>.Failure("You don't have access to view time logs for this task");

            var totalMinutes = await _unitOfWork.Repository<TimeLog>()
                .Query()
                .Where(t => t.TaskId == taskId && !t.IsDeleted && t.EndTime != null)
                .SumAsync(t => t.DurationInMinutes);

            return Result<int>.Success(totalMinutes);
        }

        public async Task<Result<TaskTimeStatsDto>> GetActiveWorkingTimeAsync(string taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<TaskTimeStatsDto>.Failure("Task not found");

            if (task.AssignedToUserId != userId && task.CreatedByUserId != userId)
                return Result<TaskTimeStatsDto>.Failure("You don't have access to view time stats for this task");

            var timeLogs = await _unitOfWork.Repository<TimeLog>()
                .Query()
                .Where(t => t.TaskId == taskId && !t.IsDeleted && t.EndTime != null)
                .ToListAsync();

            var activeWorkingTimeMinutes = timeLogs.Sum(t => t.DurationInMinutes);
            var activeHours = activeWorkingTimeMinutes / 60;
            var activeMinutes = activeWorkingTimeMinutes % 60;

            int? totalDurationMinutes = null;
            string? totalDurationFormatted = null;

            if (task.TaskStartedAt.HasValue && task.TaskCompletedAt.HasValue)
            {
                totalDurationMinutes = (int)(task.TaskCompletedAt.Value - task.TaskStartedAt.Value).TotalMinutes;
                var totalHours = totalDurationMinutes.Value / 60;
                var totalMinutes = totalDurationMinutes.Value % 60;
                totalDurationFormatted = $"{totalHours}h {totalMinutes}m";
            }

            var stats = new TaskTimeStatsDto
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                ActiveWorkingTimeMinutes = activeWorkingTimeMinutes,
                ActiveWorkingTimeFormatted = $"{activeHours}h {activeMinutes}m",
                TotalDurationMinutes = totalDurationMinutes,
                TotalDurationFormatted = totalDurationFormatted,
                TaskStartedAt = task.TaskStartedAt,
                TaskCompletedAt = task.TaskCompletedAt,
                SessionsCount = timeLogs.Count
            };

            return Result<TaskTimeStatsDto>.Success(stats);
        }
    }
}
