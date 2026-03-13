using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Team;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Team;
using SyncVerse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            var team = new Domain.Entities.Team
            {
                Name = dto.Name,
                Description = dto.Description,
                Specialization = dto.Specialization,
                Department = dto.Department,
                CreatedByManagerId = managerId
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
                CreatedAt = team.CreatedAt,
                MembersCount = 0
            };

            return Result<TeamResponseDto>.Success(response, "Team created successfully");
        }

        public async Task<Result<List<TeamResponseDto>>> GetMyTeamsAsync(string managerId)
        {
            var teams = await _unitOfWork.Repository<Domain.Entities.Team>()
                .Query()
                .Include(t => t.CreatedByManager)
                .Where(t => t.CreatedByManagerId == managerId)
                .ToListAsync();

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
                MembersCount = 0
            }).ToList();

            return Result<List<TeamResponseDto>>.Success(result);
        }

        public async Task<Result<TeamResponseDto>> GetTeamByIdAsync(string teamId, string managerId)
        {
            var team = await _unitOfWork.Repository<Domain.Entities.Team>()
                .Query()
                .Include(t => t.CreatedByManager)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                return Result<TeamResponseDto>.Failure("Team not found");

            if (team.CreatedByManagerId != managerId)
                return Result<TeamResponseDto>.Failure("Unauthorized");

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
                MembersCount = 0
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