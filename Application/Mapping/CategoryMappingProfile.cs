using AutoMapper;
using Graduation_Project.Application.DTOs.Category; 
using Graduation_Project.Domain.Entities;       

namespace Graduation_Project.Application.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CategoryResponseDto>();
        }
    }
}