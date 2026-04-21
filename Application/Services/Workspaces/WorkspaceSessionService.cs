using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Workspaces;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Workspaces;
using SyncVerse.Domain.Entities;
using System.Collections.Concurrent;

namespace SyncVerse.Application.Services.Workspaces
{
public class WorkspaceSessionService : IWorkspaceSessionService
{
    private static readonly ConcurrentDictionary<string, WorkspaceSessionDto> Sessions = new();
    private readonly UserManager<User> _userManager;
    private readonly IRepository<Workspace> _workspaceRepository;

    public WorkspaceSessionService(UserManager<User> userManager, IRepository<Workspace> workspaceRepository)
    {
        _userManager = userManager;
        _workspaceRepository = workspaceRepository;
    }

        public async Task<Result<WorkspaceSessionDto?>> GetSessionAsync(string orgCode, string userId)
        {
            var authorizationResult = await ValidateWorkspaceAccessAsync(orgCode, userId);
            if (!authorizationResult.IsSuccess)
                return Result<WorkspaceSessionDto?>.Failure(authorizationResult.Message);

            Sessions.TryGetValue(orgCode, out var session);
            return Result<WorkspaceSessionDto?>.Success(session);
        }

        public async Task<Result<WorkspaceSessionDto>> CreateSessionAsync(string orgCode, string userId, string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
                return Result<WorkspaceSessionDto>.Failure("JoinCode is required");

            var authorizationResult = await ValidateWorkspaceAccessAsync(orgCode, userId);
            if (!authorizationResult.IsSuccess)
                return Result<WorkspaceSessionDto>.Failure(authorizationResult.Message);

            var now = DateTime.UtcNow;

            var session = Sessions.AddOrUpdate(
                orgCode,
                _ => new WorkspaceSessionDto
                {
                    OrgCode = orgCode,
                    JoinCode = joinCode,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                (_, existing) =>
                {
                    existing.JoinCode = joinCode;
                    existing.UpdatedAt = now;
                    return existing;
                });

            return Result<WorkspaceSessionDto>.Success(session, "Session saved successfully");
        }

        public async Task<Result<bool>> EndSessionAsync(string orgCode, string userId)
        {
            var authorizationResult = await ValidateWorkspaceAccessAsync(orgCode, userId);
            if (!authorizationResult.IsSuccess)
                return Result<bool>.Failure(authorizationResult.Message);

            var removed = Sessions.TryRemove(orgCode, out _);
            if (!removed)
                return Result<bool>.Failure("Session not found");

            return Result<bool>.Success(true, "Session ended successfully");
        }

        private async Task<Result<bool>> ValidateWorkspaceAccessAsync(string orgCode, string userId)
        {
            if (string.IsNullOrWhiteSpace(orgCode) || string.IsNullOrWhiteSpace(userId))
                return Result<bool>.Failure("Unauthorized");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            var workspace = await _workspaceRepository.Query().FirstOrDefaultAsync(w => w.OrgCode == orgCode);
            if (workspace == null)
                return Result<bool>.Failure("Workspace not found");

            if (string.IsNullOrWhiteSpace(user.WorkspaceId) || user.WorkspaceId != workspace.Id)
                return Result<bool>.Failure("Access denied for this workspace");

            return Result<bool>.Success(true);
        }
    }
}
