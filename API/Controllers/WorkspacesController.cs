using Graduation_Project.Application.DTOs.Workspaces;
using Graduation_Project.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Policy = "ManagerOnly")]
[ApiController]
[Route("api/workspaces")]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkspaceDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _workspaceService.CreateAsync(dto, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateWorkspaceDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _workspaceService.UpdateAsync(id, dto, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _workspaceService.GetByIdAsync(id, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
