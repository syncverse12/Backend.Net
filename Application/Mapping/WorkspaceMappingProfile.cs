using AutoMapper;
using SyncVerse.Application.DTOs.Workspaces;
using SyncVerse.Domain.Entities;
using SyncVerse.Application.DTOs.Profile;

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
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.OrgCode, opt => opt.MapFrom(src => src.Workspace != null ? src.Workspace.OrgCode : null))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender));
        }
    }
}
