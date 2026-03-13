using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SyncVerse.Application.Interfaces.WorkspaceInvitation;
using SyncVerse.Application.DTOs.WorkspaceInvitation;

namespace SyncVerse.API.Controllers
{
    [ApiController]
    [Route("api/company-invitations")]
    public class CompanyInvitationController : ControllerBase
    {
        private readonly ICompanyInvitationService _invitationService;

        public CompanyInvitationController(ICompanyInvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        /// <summary>
        /// HR sends company invitation
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("send")]
        public async Task<IActionResult> SendInvitation(SendCompanyInvitationDto dto)
        {
            var hrId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _invitationService.SendInvitationAsync(dto, hrId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get invitation details by token (Public)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("details/{token}")]
        public async Task<IActionResult> GetInvitationDetails(string token)
        {
            var result = await _invitationService.GetInvitationDetailsAsync(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// ✅ Complete Profile & Connect User To Company (Requires Login)
        /// </summary>
        [Authorize] // يجب أن يكون مسجل دخول
        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile(CompleteProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _invitationService.CompleteProfileAsync(dto, userId);
            
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}