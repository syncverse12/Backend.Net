using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Category;

namespace Graduation_Project.Application.Interfaces.Task.Manager
{
    public interface ICategoryTaskService
    {
        Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, string userId);
        Task<Result<List<CategoryResponseDto>>> GetMyCategoriesAsync(string userId);
        Task<Result<bool>> DeleteAsync(int categoryId, string userId);
    }

}
