using AutoMapper;
using Graduation_Project.Application.DTOs.Auth;
using Graduation_Project.Application.DTOs.Tasks;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Domain.Models;

namespace Graduation_Project.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterRequestDto, User>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<RegisterRequestDto, User>();

           

        }
        public class TaskMappingProfile : Profile
        {
            public TaskMappingProfile()
            {
                CreateMap<TaskItem, TaskResponseDto>();
            }
        }

    }
}
