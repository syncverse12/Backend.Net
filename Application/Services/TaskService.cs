using AutoMapper;
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
                UserId = userId
            };

            await _unitOfWork.Repository<TaskItem>().AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<TaskResponseDto>.Success(
                _mapper.Map<TaskResponseDto>(task),
                "Task Created Successfully"
            );
        }

        public async Task<Result<List<TaskResponseDto>>> GetMyTasksAsync(string userId)
        {
            var tasks = await _unitOfWork.Repository<TaskItem>()
                .FindAsync(t => t.UserId == userId);

            return Result<List<TaskResponseDto>>.Success(
                _mapper.Map<List<TaskResponseDto>>(tasks),
                "Tasks Retrieved"
            );
        }

        public async Task<Result<TaskResponseDto>> UpdateAsync(
             int taskId,
            UpdateTaskDto dto,
            string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .GetByIdAsync(taskId);

            if (task == null)
            {
                return Result<TaskResponseDto>.Failure(
                    "Task not found"
                );
            }

            // Ownership Check
            if (task.UserId != userId)
            {
                return Result<TaskResponseDto>.Failure(
                    "You are not authorized to update this task"
                );
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.IsCompleted = dto.IsCompleted;

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<TaskResponseDto>.Success(
                _mapper.Map<TaskResponseDto>(task),
                "Task Updated Successfully"
            );
        }

        public async Task<Result<bool>> DeleteAsync(int taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .GetByIdAsync(taskId);

            if (task == null)
                return Result<bool>.Failure("Task not found");

            if (task.UserId != userId)
                return Result<bool>.Failure("You are not authorized to delete this task");

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

            if (task == null)
                return Result<bool>.Failure("Task not found");

            if (task.UserId != userId)
                return Result<bool>.Failure("Unauthorized");

            task.IsDeleted = false;

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task restored");
        }


    }
}
