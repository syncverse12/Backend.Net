using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Team;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Team;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Application.Services.Team
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TeamResponseDto>> CreateTeamAsync(CreateTeamDto dto, string managerId)
        {
            var manager = await _unitOfWork.Repository<User>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == managerId);

            if (manager == null)
                return Result<TeamResponseDto>.Failure("Manager not found");

            if (string.IsNullOrWhiteSpace(manager.WorkspaceId))
                return Result<TeamResponseDto>.Failure("Workspace not found for current manager");

            var team = new Domain.Entities.Team
            {
                Name = dto.Name,
                Description = dto.Description,
                Specialization = dto.Specialization,
                Department = dto.Department,
                CreatedByManagerId = managerId,
                WorkspaceId = manager.WorkspaceId
            };

            await _unitOfWork.Repository<Domain.Entities.Team>().AddAsync(team);
            await _unitOfWork.SaveChangesAsync();

            var response = new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                Specialization = team.Specialization,
                Department = team.Department,
                DepartmentDisplay = team.Department.ToString(),
                ManagerName = $"{manager.FirstName} {manager.LastName}",
                CreatedAt = team.CreatedAt,
                MembersCount = 0
            };

            return Result<TeamResponseDto>.Success(response, "Team created successfully");
        }

        public async Task<Result<List<TeamResponseDto>>> GetMyTeamsAsync(string userId, string userRole, string workspaceId, string orgCode)
        {
            // Prefer workspaceId from token claims; fallback to DB if missing
            if (string.IsNullOrWhiteSpace(workspaceId))
            {
                var currentUser = await _unitOfWork.Repository<User>()
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (currentUser == null)
                    return Result<List<TeamResponseDto>>.Failure("User not found");

                if (string.IsNullOrWhiteSpace(currentUser.WorkspaceId))
                    return Result<List<TeamResponseDto>>.Failure("Workspace not found for current user");

                workspaceId = currentUser.WorkspaceId;
            }

            var teamsQuery = _unitOfWork.Repository<Domain.Entities.Team>()
                .Query()
                .Include(t => t.CreatedByManager)
                .Include(t => t.TeamMembers)
                .Where(t => t.WorkspaceId == workspaceId);

            if (string.Equals(userRole, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                teamsQuery = teamsQuery.Where(t => t.CreatedByManagerId == userId);
            }
            else if (!string.Equals(userRole, "HR", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(userRole, "TeamLeader", StringComparison.OrdinalIgnoreCase))
            {
                teamsQuery = teamsQuery.Where(t => false);
            }

            var teams = await teamsQuery.ToListAsync();

            var result = teams.Select(t => new TeamResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Specialization = t.Specialization,
                Department = t.Department,
                DepartmentDisplay = t.Department.ToString(),
                ManagerName = $"{t.CreatedByManager.FirstName} {t.CreatedByManager.LastName}",
                CreatedAt = t.CreatedAt,
                MembersCount = t.TeamMembers.Count(tm => tm.IsActive)
            }).ToList();

            return Result<List<TeamResponseDto>>.Success(result);
        }

        public async Task<Result<TeamResponseDto>> GetTeamByIdAsync(string teamId, string userId, string userRole)
        {
            var currentUser = await _unitOfWork.Repository<User>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser == null)
                return Result<TeamResponseDto>.Failure("User not found");

            if (string.IsNullOrWhiteSpace(currentUser.WorkspaceId))
                return Result<TeamResponseDto>.Failure("Workspace not found for current user");

            var teamQuery = _unitOfWork.Repository<Domain.Entities.Team>()
                .Query()
                .Include(t => t.CreatedByManager)
                .Include(t => t.Workspace)
                .Where(t => t.Id == teamId && t.WorkspaceId == currentUser.WorkspaceId && t.CreatedByManager.WorkspaceId == currentUser.WorkspaceId);

            if (string.Equals(userRole, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                teamQuery = teamQuery.Where(t => t.CreatedByManagerId == userId);
            }
            else if (!string.Equals(userRole, "HR", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return Result<TeamResponseDto>.Failure("Unauthorized");
            }

            var team = await teamQuery.FirstOrDefaultAsync();

            if (team == null)
                return Result<TeamResponseDto>.Failure("Team not found");

            var membersCount = await _unitOfWork.Repository<TeamMember>()
                .Query()
                .Where(tm => tm.TeamId == team.Id && tm.IsActive)
                .Select(tm => tm.UserId)
                .Distinct()
                .CountAsync();

            var response = new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                Specialization = team.Specialization,
                Department = team.Department,
                DepartmentDisplay = team.Department.ToString(),
                ManagerName = $"{team.CreatedByManager.FirstName} {team.CreatedByManager.LastName}",
                CreatedAt = team.CreatedAt,
                MembersCount = membersCount
            };

            return Result<TeamResponseDto>.Success(response);
        }

        public async Task<Result<bool>> UpdateTeamAsync(UpdateTeamDto dto, string managerId)
        {
            var team = await _unitOfWork.Repository<Domain.Entities.Team>().GetByIdAsync(dto.TeamId);
            if (team == null)
                return Result<bool>.Failure("Team not found");

            if (team.CreatedByManagerId != managerId)
                return Result<bool>.Failure("Unauthorized");

            team.Name = dto.Name;
            team.Description = dto.Description;
            team.Specialization = dto.Specialization;
            team.Department = dto.Department;

            _unitOfWork.Repository<Domain.Entities.Team>().Update(team);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Team updated successfully");
        }

        public async Task<Result<bool>> DeleteTeamAsync(string teamId, string managerId)
        {
            var team = await _unitOfWork.Repository<Domain.Entities.Team>().GetByIdAsync(teamId);
            if (team == null)
                return Result<bool>.Failure("Team not found");

            if (team.CreatedByManagerId != managerId)
                return Result<bool>.Failure("Unauthorized");

            team.IsDeleted = true;

            _unitOfWork.Repository<Domain.Entities.Team>().Update(team);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Team deleted successfully");
        }

        public async Task<Result<bool>> RestoreTeamAsync(string teamId, string managerId)
        {
            var team = await _unitOfWork.Repository<Domain.Entities.Team>()
                .Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                return Result<bool>.Failure("Team not found");

            if (team.CreatedByManagerId != managerId)
                return Result<bool>.Failure("Unauthorized");

            if (!team.IsDeleted)
                return Result<bool>.Failure("Team is already active and not deleted");

            team.IsDeleted = false;

            _unitOfWork.Repository<Domain.Entities.Team>().Update(team);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Team restored successfully");
        }
    }
}
