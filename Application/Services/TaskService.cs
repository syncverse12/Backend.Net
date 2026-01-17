using AutoMapper;
using Graduation_Project.Application.Common.Pagination;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Application.Services
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

        public async Task<Result<TaskResponseDto>> CreateAsync(CreateTaskDto dto, string userId)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                UserId = userId,
                CategoryId = dto.CategoryId
            };
            
            await _unitOfWork.Repository<TaskItem>().AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<TaskResponseDto>.Success(
                _mapper.Map<TaskResponseDto>(task),
                "Task Created Successfully"
            );
        }

        public async Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(string userId, TaskQuery query)
        {
            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Category)
                .Where(t => t.UserId == userId);

            if (query.IsCompleted.HasValue)
                tasksQuery = tasksQuery.Where(t => t.IsCompleted == query.IsCompleted.Value);

            if (query.IsDeleted.HasValue)
                tasksQuery = tasksQuery.Where(t => t.IsDeleted == query.IsDeleted.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(query.Search));

            tasksQuery = query.SortBy switch
            {
            TaskSortBy.Oldest => tasksQuery.OrderBy(t => t.CreatedAt),
            TaskSortBy.Title  => tasksQuery.OrderBy(t => t.Title),
    _                 => tasksQuery.OrderByDescending(t => t.CreatedAt)
            };

            var totalCount = await tasksQuery.CountAsync();

            var tasks = await tasksQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<TaskResponseDto>>(tasks);

            return Result<PagedResult<TaskResponseDto>>.Success(new PagedResult<TaskResponseDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            });
        }

        public async Task<Result<TaskResponseDto>> UpdateAsync(int taskId, UpdateTaskDto dto, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(taskId);

            if (task == null) return Result<TaskResponseDto>.Failure("Task not found");
            if (task.UserId != userId) return Result<TaskResponseDto>.Failure("Unauthorized");

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.IsCompleted = dto.IsCompleted;
            task.CategoryId = dto.CategoryId;

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<TaskResponseDto>.Success(_mapper.Map<TaskResponseDto>(task), "Task Updated Successfully");
        }

        public async Task<Result<bool>> DeleteAsync(int taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(taskId);
            if (task == null) return Result<bool>.Failure("Task not found");
            if (task.UserId != userId) return Result<bool>.Failure("Unauthorized");

            task.IsDeleted = true;
            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task deleted successfully");
        }

        public async Task<Result<bool>> RestoreAsync(int taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id.Equals(taskId));

            if (task == null) return Result<bool>.Failure("Task not found");
            if (task.UserId != userId) return Result<bool>.Failure("Unauthorized");

            task.IsDeleted = false;
            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task restored");
        }
    }
}