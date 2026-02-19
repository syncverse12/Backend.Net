namespace Graduation_Project.API.Controllers.project.Employee
{
    using Graduation_Project.Application.DTOs.Project.Employee;
    using Graduation_Project.Application.Interfaces;
    using Graduation_Project.Application.Services.Project.Employee;
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

        // 📌 1️⃣ Get My Projects
        [HttpGet("projects")]
        public async Task<IActionResult> GetMyProjects()
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _employeeProjectService.GetMyProjectsAsync(employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // 📌 2️⃣ Get Project Details with Milestones
        [HttpGet("projects/{projectId}")]
        public async Task<IActionResult> GetProjectDetails(string projectId)
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _employeeProjectService.GetProjectDetailsAsync(projectId, employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // 📌 3️⃣ Get My Invitations
        [HttpGet("invitations")]
        public async Task<IActionResult> GetMyInvitations()
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _employeeProjectService.GetMyInvitationsAsync(employeeId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // 📌 4️⃣ Respond to Invitation
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
