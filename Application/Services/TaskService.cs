using AutoMapper;
using Graduation_Project.Application.Common.Pagination;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Domain.Enums;
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

        //CREATE
        public async Task<Result<TaskResponseDto>> CreateAsync(CreateTaskDto dto, string userId)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedByUserId = userId,       
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
        public async Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(string userId, TaskQuery query)
        {
            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Category)
                .Include(t => t.AssignedToUser) 
                .Include(t => t.CreatedByUser)
                .Where(t => t.AssignedToUserId == userId);

            if (query.IsCompleted.HasValue)
                tasksQuery = tasksQuery.Where(t => t.IsCompleted == query.IsCompleted.Value);

            if (query.IsDeleted.HasValue)
                tasksQuery = tasksQuery.Where(t => t.IsDeleted == query.IsDeleted.Value);

            if (query.Priority.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Priority == query.Priority.Value);


            if (!string.IsNullOrWhiteSpace(query.Search))
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(query.Search));

            tasksQuery = query.SortBy switch
            {
            TaskSortBy.Oldest => tasksQuery.OrderBy(t => t.CreatedAt),
            TaskSortBy.Title  => tasksQuery.OrderBy(t => t.Title),
            TaskSortBy.Priority => tasksQuery.OrderByDescending(t => t.Priority),
            _ => tasksQuery.OrderByDescending(t => t.CreatedAt),
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

        //UPDATE
        public async Task<Result<TaskResponseDto>> UpdateAsync(string taskId, UpdateTaskDto dto, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(taskId);

            if (task == null) return Result<TaskResponseDto>.Failure("Task not found");
            if (task.CreatedByUserId != userId && task.AssignedToUserId != userId) return Result<TaskResponseDto>.Failure("Unauthorized");

            if (dto.IsCompleted && !task.IsCompleted)
            {
                var hasIncompleteDependencies = await _unitOfWork.Repository<TaskDependency>()
                    .Query()
                    .Include(d => d.DependsOnTask)
                    .AnyAsync(d =>
                        d.TaskId == task.Id &&
                        !d.DependsOnTask.IsCompleted
                    );

                if (hasIncompleteDependencies)
                {
                    return Result<TaskResponseDto>.Failure(
                        "Cannot complete this task. Some dependent tasks are not completed yet."
                    );
                }
            }


            task.Title = dto.Title;
            task.Description = dto.Description;
            task.IsCompleted = dto.IsCompleted;
            task.CategoryId = dto.CategoryId;
            task.Priority = dto.Priority;
            



            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<TaskResponseDto>.Success(_mapper.Map<TaskResponseDto>(task), "Task Updated Successfully");
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
                .FindAsync(d =>
                    d.TaskId == dto.TaskId &&
                    d.DependsOnTaskId == dto.DependsOnTaskId);

            if (exists.Any())
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

    }
}