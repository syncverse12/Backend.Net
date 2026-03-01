using AutoMapper;
using SyncVerse.Application.DTOs.Auth;
using SyncVerse.Domain.Entities;


namespace SyncVerse.Application.Mapping
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
