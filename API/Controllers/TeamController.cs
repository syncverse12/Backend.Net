using Graduation_Project.Application.Interfaces;
using Graduation_Project.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Policy = "ManagerOnly")]
[ApiController]
[Route("api/team")]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpPost("invite")]
    public async Task<IActionResult> Invite(InviteTeamMemberDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _teamService.InviteMemberAsync(dto, managerId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{projectId}/members")]
    public async Task<IActionResult> GetTeamMembers(string projectId)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _teamService
            .GetProjectTeamMembersAsync(projectId, managerId);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("role")]
    public async Task<IActionResult> UpdateRole(UpdateTeamMemberRoleDto dto)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _teamService.UpdateMemberRoleAsync(dto, managerId);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(string id)
    {
        var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _teamService.RemoveMemberAsync(
            new RemoveTeamMemberDto { TeamMemberId = id },
            managerId);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

}
