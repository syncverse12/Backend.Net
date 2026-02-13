using AutoMapper;
using Graduation_Project.Application.DTOs.Milestones; 
using Graduation_Project.Domain.Entities;

namespace Graduation_Project.Application.Mapping
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