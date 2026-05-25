using AutoMapper;
using SyncVerse.Application.DTOs.TaskCategory;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Application.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<TaskCategory, TaskCategoryResponseDto>();
        }
    }
}