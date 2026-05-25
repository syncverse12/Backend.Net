using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.TaskCategory;

namespace SyncVerse.Application.Interfaces.Task.Manager
{
    public interface ICategoryTaskService
    {
        Task<Result<TaskCategoryResponseDto>> CreateAsync(CreateTaskCategoryDto dto, string userId);
        Task<Result<List<TaskCategoryResponseDto>>> GetMyCategoriesAsync(string userId);
        Task<Result<bool>> DeleteAsync(string categoryId, string userId);
    }

}
