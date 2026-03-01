using AutoMapper;
using SyncVerse.Application.DTOs.Project;
using SyncVerse.Application.DTOs.Project.Manager;
using SyncVerse.Domain.Entities;

public class ProjectMappingProfile : Profile
{
    public ProjectMappingProfile()
    {
        CreateMap<Project, ProjectResponseDto>()
            .ForMember(d => d.WorkspaceName, o => o.MapFrom(s => s.Workspace.Name));

        CreateMap<CreateProjectDto, Project>();
        CreateMap<UpdateProjectDto, Project>();
    }
}
