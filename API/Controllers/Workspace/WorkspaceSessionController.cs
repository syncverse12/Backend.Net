using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Workspaces;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Application.Interfaces.Workspaces;

namespace SyncVerse.API.Controllers.Workspace
{
    [ApiController]
    [Route("api/workspaces/{orgCode}/session")]
    [Authorize]
    public class WorkspaceSessionController : ControllerBase
    {
        private readonly IWorkspaceSessionService _workspaceSessionService;
        private readonly ICurrentUserService _currentUserService;

        public WorkspaceSessionController(
            IWorkspaceSessionService workspaceSessionService,
            ICurrentUserService currentUserService)
        {
            _workspaceSessionService = workspaceSessionService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<WorkspaceSessionDto?>>> GetSession(string orgCode)
        {
            var userId = _currentUserService.UserId!;
            var result = await _workspaceSessionService.GetSessionAsync(orgCode, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            if (result.Data == null)
                return NotFound(Result<WorkspaceSessionDto?>.Failure("Session not found"));

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<WorkspaceSessionDto>>> CreateSession(
            string orgCode,
            [FromBody] CreateWorkspaceSessionDto dto)
        {
            var userId = _currentUserService.UserId!;
            var result = await _workspaceSessionService.CreateSessionAsync(orgCode, userId, dto.JoinCode);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult<Result<bool>>> EndSession(string orgCode)
        {
            var userId = _currentUserService.UserId!;
            var result = await _workspaceSessionService.EndSessionAsync(orgCode, userId);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}
