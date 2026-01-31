using AutoMapper;
using Graduation_Project.Application.Common.Pagination;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks.Manager;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Application.Interfaces.Task.Manager;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Graduation_Project.Application.Services.Task.Manager
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //CREATE
        public async Task<Result<TaskResponseDto>> CreateAsync(
            CreateTaskDto dto,
            string managerId)
        {

            if (dto.AssignedToUserId == managerId)
                return Result<TaskResponseDto>.Failure("Manager cannot assign task to himself");

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = TaskStatus.Pending,
                CreatedByUserId = managerId,
                AssignedToUserId = dto.AssignedToUserId,
                CategoryId = dto.CategoryId?.ToString(),
                Priority = dto.Priority
            };

            await _unitOfWork.Repository<TaskItem>().AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<TaskResponseDto>.Success(
                _mapper.Map<TaskResponseDto>(task),
                "Task Created Successfully"
            );
        }


        //GET
        public async Task<Result<PagedResult<TaskResponseDto>>> GetManagerTasksAsync(
              string managerId,
              TaskQuery query)
        {
            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Category)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Where(t => t.CreatedByUserId == managerId);

            // Filters
            if (query.Status.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Status == query.Status.Value);

            if (query.Priority.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Priority == query.Priority.Value);

            if (query.IsDeleted.HasValue)
                tasksQuery = tasksQuery.Where(t => t.IsDeleted == query.IsDeleted.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(query.Search));

            tasksQuery = query.SortBy switch
            {
                TaskSortBy.Oldest => tasksQuery.OrderBy(t => t.CreatedAt),
                TaskSortBy.Title => tasksQuery.OrderBy(t => t.Title),
                TaskSortBy.Priority => tasksQuery.OrderByDescending(t => t.Priority),
                _ => tasksQuery.OrderByDescending(t => t.CreatedAt),
            };

            var totalCount = await tasksQuery.CountAsync();

            var tasks = await tasksQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return Result<PagedResult<TaskResponseDto>>.Success(
                new PagedResult<TaskResponseDto>
                {
                    Items = _mapper.Map<List<TaskResponseDto>>(tasks),
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize
                });
        }


        //UPDATE
        public async Task<Result<TaskResponseDto>> UpdateAsync(
              string taskId,
              UpdateTaskDto dto,
              string managerId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .GetByIdAsync(taskId);

            if (task == null)
                return Result<TaskResponseDto>.Failure("Task not found");

            if (task.Status == TaskStatus.Submitted)
            {
                return Result<TaskResponseDto>.Failure("Task is submitted and cannot be modified. Please Review (Accept/Reject) first.");
            }

            if (task.CreatedByUserId != managerId)
                return Result<TaskResponseDto>.Failure("Unauthorized");

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Priority = dto.Priority;
            task.CategoryId = dto.CategoryId;
            task.AssignedToUserId = dto.AssignedToUserId;

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<TaskResponseDto>.Success(
                _mapper.Map<TaskResponseDto>(task),
                "Task updated successfully"
            );
        }


        //DELETE
        public async Task<Result<bool>> DeleteAsync(string taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(taskId);
            if (task == null) return Result<bool>.Failure("Task not found");
            if (task.CreatedByUserId != userId) return Result<bool>.Failure("Unauthorized");

            task.IsDeleted = true;
            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task deleted successfully");
        }

        //RESTORE
        public async Task<Result<bool>> RestoreAsync(string taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id.Equals(taskId));

            if (task == null) return Result<bool>.Failure("Task not found");
            if (task.CreatedByUserId != userId) return Result<bool>.Failure("Unauthorized");

            task.IsDeleted = false;
            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task restored");
        }

        // ADD DEPENDENCY
        public async Task<Result<bool>> AddDependencyAsync(
              AddTaskDependencyDto dto,
              string userId)
        {
            if (dto.TaskId == dto.DependsOnTaskId)
                return Result<bool>.Failure("Task cannot depend on itself");

            var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(dto.TaskId);
            var dependsOnTask = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(dto.DependsOnTaskId);

            if (task == null || dependsOnTask == null)
                return Result<bool>.Failure("Task not found");


            if (task.CreatedByUserId != userId)
                return Result<bool>.Failure("Unauthorized");

            // Avoid repeation
            var exists = await _unitOfWork.Repository<TaskDependency>()
                        .Query()
                        .AnyAsync(d =>
                            d.TaskId == dto.TaskId &&
                            d.DependsOnTaskId == dto.DependsOnTaskId);

            if (exists)
                return Result<bool>.Failure("Dependency already exists");


            var dependency = new TaskDependency
            {
                TaskId = dto.TaskId,
                DependsOnTaskId = dto.DependsOnTaskId
            };

            await _unitOfWork.Repository<TaskDependency>()
                .AddAsync(dependency);

            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Dependency added");
        }

        public async Task<Result<bool>> ConfirmTaskAsync(
            string taskId,
            string managerId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .GetByIdAsync(taskId);

            if (task == null)
                return Result<bool>.Failure("Task not found");

            if (task.CreatedByUserId != managerId)
                return Result<bool>.Failure("Unauthorized");

            if (task.Status != TaskStatus.Submitted)
                return Result<bool>.Failure("Task is not ready for confirmation");

            task.Status = TaskStatus.Completed;
            task.ReviewedAt = DateTime.UtcNow;
            task.ReviewedByUserId = managerId;

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task confirmed as completed");
        }

        public async Task<Result<bool>> RejectTaskAsync(
            string taskId,
            string managerId,
            string comment)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .GetByIdAsync(taskId);

            if (task == null)
                return Result<bool>.Failure("Task not found");

            if (task.CreatedByUserId != managerId)
                return Result<bool>.Failure("Unauthorized");

            if (task.Status != TaskStatus.Submitted)
                return Result<bool>.Failure("Task is not submitted yet");

            task.Status = TaskStatus.Rejected;
            task.ReviewComment = comment;
            task.ReviewedAt = DateTime.UtcNow;
            task.ReviewedByUserId = managerId;

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task rejected");
        }

        public async Task<Result<ManagerTaskDashboardDto>> GetManagerDashboardAsync(string managerId)
        {
            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .IgnoreQueryFilters()
                .Where(t => t.CreatedByUserId == managerId);

            var tasks = await tasksQuery.ToListAsync();

            var statusStats = new TaskStatusStatsDto
            {
                Total = tasks.Count(t => !t.IsDeleted),
                Pending = tasks.Count(t => t.Status == TaskStatus.Pending),
                InProgress = tasks.Count(t => t.Status == TaskStatus.InProgress),
                Submitted = tasks.Count(t => t.Status == TaskStatus.Submitted),
                Completed = tasks.Count(t => t.Status == TaskStatus.Completed),
                Rejected = tasks.Count(t => t.Status == TaskStatus.Rejected)
            };

            var tasksPerEmployee = tasks
                .Where(t => t.AssignedToUserId != null && !t.IsDeleted)
                .GroupBy(t => new { t.AssignedToUserId, EmployeeName = (t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName).Trim() })
                .Select(g => new EmployeeTaskStatsDto
                {
                    EmployeeId = g.Key.AssignedToUserId,
                    EmployeeName = g.Key.EmployeeName,
                    TasksCount = g.Count()
                })
                .ToList();

            var dashboard = new ManagerTaskDashboardDto
            {
                StatusStats = statusStats,
                TasksPerEmployee = tasksPerEmployee
            };

            return Result<ManagerTaskDashboardDto>.Success(dashboard);
        }
    }
}