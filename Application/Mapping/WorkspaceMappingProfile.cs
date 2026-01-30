using AutoMapper;
using Graduation_Project.Application.DTOs.Workspaces;
using Graduation_Project.Domain.Entities;

namespace Graduation_Project.Application.Mapping
{
    public class WorkspaceMappingProfile : Profile
    {
        public WorkspaceMappingProfile()
        {
            CreateMap<Workspace, WorkspaceResponseDto>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.UserName));

            CreateMap<CreateWorkspaceDto, Workspace>();
            CreateMap<UpdateWorkspaceDto, Workspace>();
        }
    }
}
