using Graduation_Project.Application.DTOs.Project.Manager;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Policy = "ManagerOnly")]
[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _projectService.CreateAsync(dto, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateProjectDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _projectService.UpdateAsync(id, dto, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> GetById(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _projectService.GetByIdAsync(id, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("workspace/{workspaceId}")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> GetByWorkspace(string workspaceId)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _projectService.GetByWorkspaceAsync(workspaceId, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // DELETE PROJECT (Soft Delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _projectService.DeleteProjectAsync(id, managerId); 
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // RESTORE PROJECT
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> Restore(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _projectService.RestoreProjectAsync(id, managerId); 
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // INVITATION
    [HttpPost("{projectId}/invite")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<IActionResult> Invite(string projectId, InviteEmployeeDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _projectService.InviteEmployeeAsync(projectId, dto, managerId);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

}
