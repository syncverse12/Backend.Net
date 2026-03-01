using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Category;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Task.Manager;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Application.Services.Task.Manager
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
            var nameExists = await _unitOfWork.Repository<Category>().Query()
        .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && !c.IsDeleted);

            if (nameExists)
                return Result<CategoryResponseDto>.Failure("This category type already exists in the system.");

            var category = new Category
            {
                Name = dto.Name,
                UserId = userId 
            };

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryResponseDto>.Success(
                _mapper.Map<CategoryResponseDto>(category),
                "Category created successfully"
            );
        }

        public async Task<Result<List<CategoryResponseDto>>> GetMyCategoriesAsync(string userId)
        {
            var categories = await _unitOfWork.Repository<Category>()
                .Query()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
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