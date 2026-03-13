using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Auth;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.WorkspaceInvitation;
using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;
using SyncVerse.API.JwtFeatuers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using SyncVerse.Application.DTOs.WorkspaceInvitation;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SyncVerse.Application.Services.WorkspaceInvitation
{
    public class CompanyInvitationService : ICompanyInvitationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly JwtHandler _jwtHandler;
        private readonly IConfiguration _configuration;

        public CompanyInvitationService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            UserManager<User> userManager,
            JwtHandler jwtHandler,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _userManager = userManager;
            _jwtHandler = jwtHandler;
            _configuration = configuration;
        }

        public async Task<Result<bool>> SendInvitationAsync(SendCompanyInvitationDto dto, string hrId)
        {
            var hr = await _userManager.FindByIdAsync(hrId);
            if (hr == null) return Result<bool>.Failure("HR user not found");

            var team = await _unitOfWork.Repository<Domain.Entities.Team>().GetByIdAsync(dto.TeamId);
            if (team == null) return Result<bool>.Failure("Team not found");

            var token = GenerateSecureToken();

            var invitation = new CompanyInvitation
            {
                Email = dto.Email,
                TeamId = dto.TeamId,
                SeniorityLevel = dto.SeniorityLevel,
                Role = dto.Role,
                InvitationToken = token,
                SentByHRId = hrId,
                SentAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Status = InvitationStatus.Pending
            };

            await _unitOfWork.Repository<CompanyInvitation>().AddAsync(invitation);
            await _unitOfWork.SaveChangesAsync();

            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            
            // ✅ توجيه المستخدم لصفحة الـ Register بدلاً من Join-Company
            var invitationLink = $"{frontendUrl}/register?token={token}&email={dto.Email}";

            var emailBody = $"<a href='{invitationLink}' class='btn'>Complete Registration</a>"; // تفاصيل الإيميل كما هي لديك
            await _emailService.SendAsync(dto.Email, "Invitation to Join SyncVerse", emailBody);

            return Result<bool>.Success(true, "Company invitation sent successfully");
        }

        public async Task<Result<InvitationDetailsDto>> GetInvitationDetailsAsync(string token)
        {
            var invitation = await _unitOfWork.Repository<CompanyInvitation>()
                .Query()
                .Include(i => i.Team)
                .Include(i => i.SentByHR)
                .FirstOrDefaultAsync(i => i.InvitationToken == token);

            if (invitation == null)
                return Result<InvitationDetailsDto>.Failure("Invalid invitation link");

            if (invitation.Status != InvitationStatus.Pending)
                return Result<InvitationDetailsDto>.Failure("This invitation has already been used");

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                await _unitOfWork.SaveChangesAsync();
                return Result<InvitationDetailsDto>.Failure("This invitation has expired");
            }

            var details = new InvitationDetailsDto
            {
                Email = invitation.Email,
                TeamId = invitation.TeamId,
                TeamName = invitation.Team.Name,
                TeamDescription = invitation.Team.Description,
                
                Department = invitation.Team.Department,
                DepartmentDisplay = invitation.Team.Department.ToString(),
                
                SeniorityLevel = invitation.SeniorityLevel,
                SeniorityLevelDisplay = invitation.SeniorityLevel.ToString(),
                
                Role = invitation.Role,  
                RoleDisplay = invitation.Role.ToString(),
                
                HRName = $"{invitation.SentByHR.FirstName} {invitation.SentByHR.LastName}",
                ExpiresAt = invitation.ExpiresAt,
                IsValid = true
            };

            return Result<InvitationDetailsDto>.Success(details);
        }

        // ✅ استكمال الملف للمستخدم وتسجيله في الفريق وتفعيل صلاحياته
        public async Task<Result<AuthResponseDto>> CompleteProfileAsync(CompleteProfileDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result<AuthResponseDto>.Failure("User not found");

            var invitation = await _unitOfWork.Repository<CompanyInvitation>()
                .Query()
                .Include(i => i.Team)
                .FirstOrDefaultAsync(i => i.InvitationToken == dto.Token);

            if (invitation == null) return Result<AuthResponseDto>.Failure("Invalid invitation link");
            if (invitation.Status != InvitationStatus.Pending) return Result<AuthResponseDto>.Failure("Invitation already used or expired");

            // تحديث بيانات الموظف
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            user.SeniorityLevel = invitation.SeniorityLevel;
            user.Department = invitation.Team.Department;
            await _userManager.UpdateAsync(user);

            // منح الرتب والأدوار
            await _userManager.AddToRoleAsync(user, invitation.Role.ToString());
            await ReplaceUserClaimAsync(user, "Department", user.Department.ToString());
            await ReplaceUserClaimAsync(user, "SeniorityLevel", user.SeniorityLevel.ToString());

            // إكمال الدعوة
            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            // توليد توكن جديد للمستخدم بالصلاحيات المضافة
            var roles = await _userManager.GetRolesAsync(user);
            var tokenInfo = _jwtHandler.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = tokenInfo.Token,
                Expiration = tokenInfo.Expiration,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                },
                Message = "Profile completed and assigned to the team successfully"
            });
        }

        private string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        // ✅ هنا تحديد نوع الإرجاع كـ Task صريح بدلاً من خطأ الخلط بين Task الخاصة بكيان آخر (إن وجدت)
        private async System.Threading.Tasks.Task ReplaceUserClaimAsync(User user, string claimType, string claimValue)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var existing = claims.FirstOrDefault(c => c.Type == claimType);
            if (existing != null)
                await _userManager.RemoveClaimAsync(user, existing);
            await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue));
        }
    }
}