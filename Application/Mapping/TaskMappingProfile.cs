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
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "No Category"))

               .ForMember(dest => dest.AssignedToUserName,
                    opt => opt.MapFrom(src => src.AssignedToUser != null
                    ? (src.AssignedToUser.FirstName + " " + src.AssignedToUser.LastName).Trim()
                    : "Unassigned"))

               .ForMember(dest => dest.CreatedByUserName,
                    opt => opt.MapFrom(src => src.CreatedByUser != null
                    ? (src.CreatedByUser.FirstName + " " + src.CreatedByUser.LastName).Trim()
                    : "System Manager"))

               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))

               .ForMember(dest => dest.IsCompleted,
                    opt => opt.MapFrom(src => src.Status == TaskStatus.Completed));
        }
    }

}
