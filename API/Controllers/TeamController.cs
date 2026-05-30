using SyncVerse.Application.DTOs.Team;
using SyncVerse.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SyncVerse.Application.Interfaces.Team;

namespace SyncVerse.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/teams")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        /// ✅ CREATE - Create new team
        [HttpPost]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> CreateTeam(CreateTeamDto dto)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.CreateTeamAsync(dto, managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ READ - Get all my teams
        [HttpGet("my-teams")]
        [Authorize(Roles = "HR,Manager,TeamLeader")]
        public async Task<IActionResult> GetMyTeams()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var workspaceId = User.FindFirstValue("WorkspaceId") ?? string.Empty;
            var orgCode = User.FindFirstValue("orgCode") ?? string.Empty;
            var result = await _teamService.GetMyTeamsAsync(userId, userRole, workspaceId, orgCode);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ READ - Get team by ID
        [HttpGet("{teamId}")]
        [Authorize]
        public async Task<IActionResult> GetTeamById(string teamId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var result = await _teamService.GetTeamByIdAsync(teamId, userId, userRole);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ UPDATE - Update team
        [HttpPut]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> UpdateTeam(UpdateTeamDto dto)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.UpdateTeamAsync(dto, managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ DELETE - Delete team
        [HttpDelete("{teamId}")]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> DeleteTeam(string teamId)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.DeleteTeamAsync(teamId, managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ RESTORE - Restore a softly deleted team
        [HttpPut("{teamId}/restore")]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> RestoreTeam(string teamId)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.RestoreTeamAsync(teamId, managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
