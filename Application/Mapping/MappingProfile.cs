using AutoMapper;
using Graduation_Project.Application.DTOs.Auth;
using Graduation_Project.Domain.Entities;


namespace Graduation_Project.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterDto, User>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<RegisterDto, User>();

           

        }
        

    }
}
