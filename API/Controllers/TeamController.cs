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
}
