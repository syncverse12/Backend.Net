using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Graduation_Project.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvitationService _invitationService;

        public TeamService(
            IUnitOfWork unitOfWork,
            IInvitationService invitationService)
        {
            _unitOfWork = unitOfWork;
            _invitationService = invitationService;
        }

        public async Task<Result<List<TeamMemberResponseDto>>> GetProjectTeamMembersAsync(string projectId,string managerId)
        {
            var project = await _unitOfWork.Repository<Domain.Entities.Project>()
                .GetByIdAsync(projectId);

            if (project == null)
                return Result<List<TeamMemberResponseDto>>.Failure("Project not found");

            if (project.CreatedByUserId != managerId)
                return Result<List<TeamMemberResponseDto>>.Failure("Unauthorized");

            var members = await _unitOfWork.Repository<TeamMember>()
                 .Query() 
                 .Include(m => m.User) 
                 .Where(m => m.ProjectId == projectId)
                 .ToListAsync();

            var result = members.Select(m => new TeamMemberResponseDto
            {
                TeamMemberId = m.Id,
                UserId = m.UserId,
                UserEmail = m.User?.Email ?? "N/A", 
                Role = m.Role,
                IsActive = m.IsActive
            }).ToList();

            return Result<List<TeamMemberResponseDto>>.Success(result);
        }


        public async Task<Result<bool>> InviteMemberAsync(
            InviteTeamMemberDto dto,
            string managerId)
        {
            var project = await _unitOfWork.Repository<Domain.Entities.Project>()
                .GetByIdAsync(dto.ProjectId);

            if (project == null)
                return Result<bool>.Failure("Project not found");

            if (project.CreatedByUserId != managerId)
                return Result<bool>.Failure("Unauthorized");

            var user = await _unitOfWork.Repository<User>()
                .FindAsync(u => u.Email == dto.UserEmail);

            if (user == null)
                return Result<bool>.Failure("User not found");

            var isAlreadyMember = await _unitOfWork.Repository<TeamMember>()
                .FindAsync(m => m.ProjectId == dto.ProjectId && m.UserId == user.Id);

            if (isAlreadyMember != null)
                return Result<bool>.Failure("User is already a member of this project");

            var member = new TeamMember
            {
                ProjectId = dto.ProjectId,
                UserId = user.Id, 
                Role = dto.Role,
                IsActive = false
            };

            await _unitOfWork.Repository<TeamMember>().AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            await _invitationService.SendInvitationAsync(
                dto.UserEmail,
                project.Name);

            return Result<bool>.Success(true, "Invitation sent successfully");
        }

        public async Task<Result<bool>> UpdateMemberRoleAsync(
            UpdateTeamMemberRoleDto dto,
            string managerId)
        {
            var member = await _unitOfWork.Repository<TeamMember>()
                .Query()
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.Id == dto.TeamMemberId);

            if (member == null)
                return Result<bool>.Failure("Team member not found");

            if (member.Project.CreatedByUserId != managerId)
                return Result<bool>.Failure("Unauthorized");

            member.Role = dto.Role;

            _unitOfWork.Repository<TeamMember>().Update(member);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Role updated successfully");
        }

        public async Task<Result<bool>> RemoveMemberAsync(
             RemoveTeamMemberDto dto,
             string managerId)
        {
            var member = await _unitOfWork.Repository<TeamMember>()
                .Query()
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.Id == dto.TeamMemberId);

            if (member == null)
                return Result<bool>.Failure("Team member not found");

            if (member.Project.CreatedByUserId != managerId)
                return Result<bool>.Failure("Unauthorized");

            _unitOfWork.Repository<TeamMember>().Delete(member);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Team member removed successfully");
        }


    }
}