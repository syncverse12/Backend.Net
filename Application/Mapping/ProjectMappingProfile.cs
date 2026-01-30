using AutoMapper;
using Graduation_Project.Application.DTOs.Project;
using Graduation_Project.Domain.Entities;

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
