using AutoMapper;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Project;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProjectService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProjectResponseDto>> CreateAsync(
    CreateProjectDto dto,
    string managerId)
    {
        if (dto.EndDate < dto.StartDate)
            return Result<ProjectResponseDto>.Failure("Invalid project timeline");

        var workspace = await _unitOfWork.Repository<Workspace>()
            .GetByIdAsync(dto.WorkspaceId);

        if (workspace == null || workspace.CreatedByUserId != managerId)
            return Result<ProjectResponseDto>.Failure("Workspace not found or unauthorized");

        var project = _mapper.Map<Project>(dto);
        project.CreatedByUserId = managerId;

        await _unitOfWork.Repository<Project>().AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProjectResponseDto>.Success(
            _mapper.Map<ProjectResponseDto>(project),
            "Project created successfully");
    }


    public async Task<Result<ProjectResponseDto>> UpdateAsync(
    string projectId,
    UpdateProjectDto dto,
    string managerId)
    {
        if (dto.EndDate < dto.StartDate)
            return Result<ProjectResponseDto>.Failure("Invalid project timeline");

        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .IgnoreQueryFilters()
            .Include(p => p.Workspace)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null || project.IsDeleted)
            return Result<ProjectResponseDto>.Failure("Project not found");

        if (project.Workspace.CreatedByUserId != managerId)
            return Result<ProjectResponseDto>.Failure("Unauthorized");

        _mapper.Map(dto, project);

        _unitOfWork.Repository<Project>().Update(project);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProjectResponseDto>.Success(
            _mapper.Map<ProjectResponseDto>(project),
            "Project updated successfully");
    }
}