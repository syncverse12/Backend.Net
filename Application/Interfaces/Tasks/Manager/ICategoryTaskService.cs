using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Category;

namespace SyncVerse.Application.Interfaces.Task.Manager
{
    public interface ICategoryTaskService
    {
        Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, string userId);
        Task<Result<List<CategoryResponseDto>>> GetMyCategoriesAsync(string userId);
        Task<Result<bool>> DeleteAsync(string categoryId, string userId);
    }

}
