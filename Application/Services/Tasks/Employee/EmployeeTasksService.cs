using AutoMapper;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Graduation_Project.Application.DTOs.Tasks.Manager;
using Graduation_Project.Application.Interfaces.Task.Manager;
using System.Threading.Tasks;
using Graduation_Project.Application.Interfaces.Tasks.Employee;
using Graduation_Project.Application.DTOs.Tasks.Employee;

namespace Graduation_Project.Application.Services.Task.Employee
{
    public class EmployeeTaskService : IEmployeeTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public EmployeeTaskService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(string userId, TaskQuery query)
        {
            var employeeId = string.IsNullOrEmpty(userId) ? _currentUser.UserId : userId;

            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Category)
                .Where(t =>
                    t.AssignedToUserId == employeeId &&
                    !t.IsDeleted);

            if (query.Status.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Status == query.Status.Value);

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
                    PageNumber = query.Page,
                    PageSize = query.PageSize
                });
        }

        public async Task<Result<EmployeeTaskDetailsDto>> GetTaskDetailsAsync(string taskId, string userId)
        {
            var employeeId = string.IsNullOrEmpty(userId) ? _currentUser.UserId : userId;

            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Project)
                .Include(t => t.Milestone)
                .Include(t => t.Category)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.ReviewedByUser)
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<EmployeeTaskDetailsDto>.Failure("Task not found");

            if (task.AssignedToUserId != employeeId)
                return Result<EmployeeTaskDetailsDto>.Failure("Unauthorized: You don't have access to this task");

            var details = new EmployeeTaskDetailsDto
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                ProjectName = task.Project?.Name,
                ProjectId = task.ProjectId,
                MilestoneName = task.Milestone?.Name,
                MilestoneId = task.MilestoneId,
                CategoryName = task.Category?.Name,
                CategoryId = task.CategoryId,
                ReviewComment = task.ReviewComment,
                ReviewedAt = task.ReviewedAt,
                CreatedByName = task.CreatedByUser?.UserName ?? "Unknown",
                AssignedToName = task.AssignedToUser?.UserName ?? "Unknown"
            };

            return Result<EmployeeTaskDetailsDto>.Success(details);
        }

        public async Task<Result<bool>> StartTaskAsync(string taskId, string userId)
        {
            var employeeId = string.IsNullOrEmpty(userId) ? _currentUser.UserId : userId;

            var task = await _unitOfWork.Repository<TaskItem>()
                .GetByIdAsync(taskId);

            if (task == null)
                return Result<bool>.Failure("Task not found");

            if (task.AssignedToUserId != employeeId)
                return Result<bool>.Failure("Unauthorized");

            if (task.Status != TaskStatus.Pending)
                return Result<bool>.Failure("Task cannot be started");

            task.Status = TaskStatus.InProgress;

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task started");
        }

        public async Task<Result<bool>> SubmitTaskAsync(string taskId, string userId)
        {
            var employeeId = string.IsNullOrEmpty(userId) ? _currentUser.UserId : userId;

            var task = await _unitOfWork.Repository<TaskItem>()
                .GetByIdAsync(taskId);

            if (task == null)
                return Result<bool>.Failure("Task not found");

            if (task.AssignedToUserId != employeeId)
                return Result<bool>.Failure("Unauthorized");

            if (task.Status != TaskStatus.InProgress)
                return Result<bool>.Failure("Task must be in progress");

            task.Status = TaskStatus.Submitted;

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task submitted for review");
        }
    }

}
