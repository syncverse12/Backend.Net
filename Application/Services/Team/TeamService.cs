using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
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

        public async System.Threading.Tasks.Task<Result<bool>> InviteMemberAsync(
            InviteTeamMemberDto dto,
            string managerId)
        {
            // 1. التأكد من وجود المشروع وصلاحية المدير
            var project = await _unitOfWork.Repository<Project>()
                .GetByIdAsync(dto.ProjectId);

            if (project == null)
                return Result<bool>.Failure("Project not found");

            if (project.CreatedByUserId != managerId)
                return Result<bool>.Failure("Unauthorized");

            // 2. البحث عن المستخدم (الآن يرجع كائن واحد User? وليس List)
            var user = await _unitOfWork.Repository<User>()
                .FindAsync(u => u.Email == dto.UserEmail);

            // 3. التصحيح: نتحقق إذا كان null (بدلاً من Any)
            if (user == null)
                return Result<bool>.Failure("User not found");

            // 4. فحص إذا كان العضو مضافاً بالفعل للفريق (منع التكرار)
            var isAlreadyMember = await _unitOfWork.Repository<TeamMember>()
                .FindAsync(m => m.ProjectId == dto.ProjectId && m.UserId == user.Id);

            if (isAlreadyMember != null)
                return Result<bool>.Failure("User is already a member of this project");

            // 5. إنشاء سجل العضو الجديد
            var member = new TeamMember
            {
                ProjectId = dto.ProjectId,
                UserId = user.Id, // 👈 التصحيح: نستخدم user.Id مباشرة (بدون First)
                Role = dto.Role,
                IsActive = false
            };

            await _unitOfWork.Repository<TeamMember>().AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            // 6. إرسال الدعوة (الـ Mock أو الحقيقية لاحقاً)
            await _invitationService.SendInvitationAsync(
                dto.UserEmail,
                project.Name);

            return Result<bool>.Success(true, "Invitation sent successfully");
        }
    }
}