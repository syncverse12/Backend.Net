using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Category;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Application.Interfaces.Task.Manager;
using Graduation_Project.Domain.Entities;

namespace Graduation_Project.Application.Services.Task.Manager
{
    public class CategoryTaskService : ICategoryTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryTaskService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, string userId)
        {
            var category = new Category
            {
                Name = dto.Name,
                UserId = userId
            };

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryResponseDto>.Success(
                _mapper.Map<CategoryResponseDto>(category),
                "Category created"
            );
        }

        public async Task<Result<List<CategoryResponseDto>>> GetMyCategoriesAsync(string userId)
        {
            var categories = await _unitOfWork.Repository<Category>()
                .Query()
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .ToListAsync();

            return Result<List<CategoryResponseDto>>.Success(
                _mapper.Map<List<CategoryResponseDto>>(categories)
            );
        }

        public async Task<Result<bool>> DeleteAsync(string categoryId, string userId)
        {
            var category = await _unitOfWork.Repository<Category>()
                .GetByIdAsync(categoryId);

            if (category == null)
                return Result<bool>.Failure("Category not found");

            if (category.UserId != userId)
                return Result<bool>.Failure("Unauthorized");

            category.IsDeleted = true; 

            _unitOfWork.Repository<Category>().Update(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Category deleted");
        }
    }
}