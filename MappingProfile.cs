using AutoMapper;
using Graduation_Project.DTOs;
using Graduation_Project.Models;

namespace Graduation_Project
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserForRegisterationDTO, User>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(src => src.Email));
        }
    }
}
