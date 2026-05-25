using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.TaskCategory;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Task.Manager;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Application.Services.Task.Manager
{
    public class CategoryTaskService : ICategoryTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private static readonly string[] DefaultTaskCategoryNames =
        {
            "Backend",
            "Ui_Ux",
            "Frontend",
            "AI",
            "Flutter",
            "Game",
            "DevOps",
            "Cloud",
            "CyberSecurity",
            "Testing",
            "Business",
        };

        public CategoryTaskService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<TaskCategoryResponseDto>> CreateAsync(CreateTaskCategoryDto dto, string userId)
        {
            var normalizedName = dto.Name.Trim();

            var isDefaultCategory = DefaultTaskCategoryNames.Any(name =>
                string.Equals(name, normalizedName, StringComparison.OrdinalIgnoreCase));

            var nameExists = await _unitOfWork.Repository<TaskCategory>().Query()
                .AnyAsync(c => c.Name.ToLower() == normalizedName.ToLower() && !c.IsDeleted);

            if (isDefaultCategory || nameExists)
                return Result<TaskCategoryResponseDto>.Failure("This category type already exists in the system.");

            var category = new TaskCategory
            {
                Name = normalizedName,
                UserId = userId
            };

            await _unitOfWork.Repository<TaskCategory>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<TaskCategoryResponseDto>.Success(
                _mapper.Map<TaskCategoryResponseDto>(category),
                "Category created successfully"
            );
        }

        public async Task<Result<List<TaskCategoryResponseDto>>> GetMyCategoriesAsync(string userId)
        {
            var categories = await _unitOfWork.Repository<TaskCategory>()
                .Query()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var existingNames = categories
                .Select(c => c.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var defaultName in DefaultTaskCategoryNames)
            {
                if (!existingNames.Contains(defaultName))
                {
                    categories.Add(new TaskCategory
                    {
                        Name = defaultName,
                        UserId = userId
                    });
                }
            }

            var orderedCategories = categories
                .OrderBy(c => c.Name)
                .ToList();

            return Result<List<TaskCategoryResponseDto>>.Success(
                _mapper.Map<List<TaskCategoryResponseDto>>(orderedCategories)
            );
        }

        public async Task<Result<bool>> DeleteAsync(string categoryId, string userId)
        {
            var category = await _unitOfWork.Repository<TaskCategory>()
                .GetByIdAsync(categoryId);

            if (category == null)
                return Result<bool>.Failure("Category not found");

            var isDefaultCategory = DefaultTaskCategoryNames.Any(name =>
                string.Equals(name, category.Name, StringComparison.OrdinalIgnoreCase));

            if (isDefaultCategory)
                return Result<bool>.Failure("Default categories cannot be deleted.");

            if (category.UserId != userId)
                return Result<bool>.Failure("Unauthorized");

            category.IsDeleted = true;

            _unitOfWork.Repository<TaskCategory>().Update(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Category deleted");
        }
    }
}