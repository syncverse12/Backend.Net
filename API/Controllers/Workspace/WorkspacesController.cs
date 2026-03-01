using SyncVerse.Application.DTOs.Workspaces;
using SyncVerse.Application.Interfaces;
using SyncVerse.Domain.Entities;
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

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkspaceDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _workspaceService.CreateAsync(dto, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateWorkspaceDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _workspaceService.UpdateAsync(id, dto, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _workspaceService.GetByIdAsync(id, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _workspaceService.GetAllAsync(managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _workspaceService.DeleteAsync(id, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // RESTORE
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> Restore(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _workspaceService.RestoreAsync(id, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
