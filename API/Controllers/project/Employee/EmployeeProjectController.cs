namespace SyncVerse.API.Controllers.project.Employee
{
    using SyncVerse.Application.DTOs.Project.Employee;
    using SyncVerse.Application.Interfaces;
    using SyncVerse.Application.Services.Project.Employee;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    [Authorize(Policy = "EmployeeOnly")]
    [ApiController]
    [Route("api/employee")]
    public class EmployeeProjectController : ControllerBase
    {
        private readonly IEmployeeProjectService _employeeProjectService;

        public EmployeeProjectController(IEmployeeProjectService EmployeeProjectService)
        {
            _employeeProjectService = EmployeeProjectService;
        }

        // Get My Projects
        [HttpGet("projects")]
        public async Task<IActionResult> GetMyProjects()
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _employeeProjectService.GetMyProjectsAsync(employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // Get Project Details with Milestones
        [HttpGet("projects/{projectId}")]
        public async Task<IActionResult> GetProjectDetails(string projectId)
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _employeeProjectService.GetProjectDetailsAsync(projectId, employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // Get My Invitations
        [HttpGet("invitations")]
        public async Task<IActionResult> GetMyInvitations()
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _employeeProjectService.GetMyInvitationsAsync(employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // Respond to Invitation
        [HttpPost("invitations/{invitationId}/respond")]
        public async Task<IActionResult> Respond(
            string invitationId,
            RespondInvitationDto dto)
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _employeeProjectService
                .RespondToInvitationAsync(invitationId, dto, employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

}
