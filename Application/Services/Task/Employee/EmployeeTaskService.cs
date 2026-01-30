using AutoMapper;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Application.Interfaces.Task.Employee;
using Graduation_Project.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Graduation_Project.Application.DTOs.Tasks.Manager;
using Graduation_Project.Application.Interfaces.Task.Manager;

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

        public async Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(TaskQuery query)
        {
            var employeeId = _currentUser.UserId;

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

        public async Task<Result<bool>> StartTaskAsync(string taskId)
        {
            var employeeId = _currentUser.UserId;

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

        public async Task<Result<bool>> SubmitTaskAsync(string taskId)
        {
            var employeeId = _currentUser.UserId;

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
