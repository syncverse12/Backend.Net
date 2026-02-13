using Graduation_Project.Application.DTOs.Milestones;
using Graduation_Project.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Policy = "ManagerOnly")]
[ApiController]
[Route("api/milestones")]
public class MilestonesController : ControllerBase
{
    private readonly IMilestoneService _milestoneService;

    public MilestonesController(IMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMilestoneDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _milestoneService.CreateAsync(dto, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateMilestoneDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _milestoneService.UpdateAsync(id, dto, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _milestoneService.DeleteAsync(id, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _milestoneService.GetByIdAsync(id, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectMilestones(string projectId)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _milestoneService.GetProjectMilestonesAsync(projectId, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> Restore(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _milestoneService.RestoreMilestoneAsync(id, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
