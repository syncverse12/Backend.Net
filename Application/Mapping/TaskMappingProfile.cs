using AutoMapper;
using Graduation_Project.Application.DTOs.Tasks.Manager;
using Graduation_Project.Domain.Entities;

namespace Graduation_Project.Application.Mapping
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<TaskItem, TaskResponseDto>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.AssignedToUserName,
                    opt => opt.MapFrom(src => src.AssignedToUser.UserName))
                .ForMember(dest => dest.CreatedByUserName,
                    opt => opt.MapFrom(src => src.CreatedByUser.UserName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }

}
