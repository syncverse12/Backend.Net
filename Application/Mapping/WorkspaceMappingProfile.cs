using AutoMapper;
using SyncVerse.Application.DTOs.Workspaces;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Application.Mapping
{
    public class WorkspaceMappingProfile : Profile
    {
        public WorkspaceMappingProfile()
        {
            CreateMap<Workspace, WorkspaceResponseDto>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.UserName))
                .ForMember(dest => dest.OrgCode, opt => opt.MapFrom(src => src.OrgCode));

            CreateMap<CreateWorkspaceDto, Workspace>();
            CreateMap<UpdateWorkspaceDto, Workspace>();
        }
    }
}
