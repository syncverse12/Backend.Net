using AutoMapper;
using SyncVerse.Application.DTOs.Milestones; 
using SyncVerse.Domain.Entities;

namespace SyncVerse.Application.Mapping
{
    public class MilestoneMappingProfile : Profile
    {
        public MilestoneMappingProfile()
        {
            CreateMap<CreateMilestoneDto, Milestone>();
            CreateMap<UpdateMilestoneDto, Milestone>();

            CreateMap<Milestone, MilestoneResponseDto>()
                .ForMember(dest => dest.ProjectName,
                           opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null));
        }
    }
}