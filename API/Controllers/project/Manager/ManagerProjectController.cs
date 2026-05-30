using SyncVerse.Application.DTOs.Project.Manager;
using SyncVerse.Application.Interfaces;
using SyncVerse.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "Manager,ProjectManager,Admin")]
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
    public async Task<IActionResult> GetById(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _projectService.GetByIdAsync(id, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("workspace/{workspaceId}")]
    public async Task<IActionResult> GetByWorkspaceForManager(string workspaceId)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _projectService.GetByWorkspaceForManagerAsync(workspaceId, managerId);
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
    public async Task<IActionResult> Invite(string projectId, InviteEmployeeDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _projectService.InviteEmployeeAsync(projectId, dto, managerId);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET: api/projects/{projectId}/accepted-employees
    [HttpGet("{projectId}/accepted-employees")]
    public async Task<IActionResult> GetAcceptedEmployeeNames(string projectId)
    {
        var employees = await _projectService.GetAcceptedEmployeesAsync(projectId);
        return Ok(employees);
    }
}
