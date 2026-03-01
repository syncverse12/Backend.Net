using AutoMapper;
using SyncVerse.Application.DTOs.Category; 
using SyncVerse.Domain.Entities;       

namespace SyncVerse.Application.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CategoryResponseDto>();
        }
    }
}