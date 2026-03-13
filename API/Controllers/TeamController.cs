using SyncVerse.Application.DTOs.Team;
using SyncVerse.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SyncVerse.Application.Interfaces.Team;

namespace SyncVerse.API.Controllers
{
    [Authorize(Policy = "ManagerOnly")]
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
        public async Task<IActionResult> CreateTeam(CreateTeamDto dto)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.CreateTeamAsync(dto, managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ READ - Get all my teams
        [HttpGet("my-teams")]
        public async Task<IActionResult> GetMyTeams()
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.GetMyTeamsAsync(managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ READ - Get team by ID
        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetTeamById(string teamId)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.GetTeamByIdAsync(teamId, managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ UPDATE - Update team
        [HttpPut]
        public async Task<IActionResult> UpdateTeam(UpdateTeamDto dto)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.UpdateTeamAsync(dto, managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ DELETE - Delete team
        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(string teamId)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.DeleteTeamAsync(teamId, managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// ✅ RESTORE - Restore a softly deleted team
        [HttpPut("{teamId}/restore")]
        public async Task<IActionResult> RestoreTeam(string teamId)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _teamService.RestoreTeamAsync(teamId, managerId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}