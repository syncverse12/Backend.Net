using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Auth;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;
using SyncVerse.API.JwtFeatuers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Security.Claims;
using SyncVerse.Application.Interfaces.Storage;
using SyncVerse.Application.DTOs.CompanyInvitation;
using SyncVerse.Application.Interfaces.WorkspaceInvitation;
using Microsoft.Extensions.Configuration;

namespace SyncVerse.Application.Services.WorkspaceInvitation
{
    public class CompanyInvitationService : ICompanyInvitationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly JwtHandler _jwtHandler;
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorageService;

        public CompanyInvitationService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            UserManager<User> userManager,
            JwtHandler jwtHandler,
            IConfiguration configuration,
            IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _userManager = userManager;
            _jwtHandler = jwtHandler;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<SendCompanyInvitationResponseDto>> SendInvitationAsync(SendCompanyInvitationDto dto, string hrId)
        {
            var hr = await _userManager.FindByIdAsync(hrId);
            if (hr == null) return Result<SendCompanyInvitationResponseDto>.Failure("HR user not found");

            var team = await _unitOfWork.Repository<Domain.Entities.Team>()
                .Query()
                .Include(t => t.CreatedByManager)
                .FirstOrDefaultAsync(t => t.Id == dto.TeamId);

            if (team == null) return Result<SendCompanyInvitationResponseDto>.Failure("Team not found");

            var workspace = team.Workspace;
            if (workspace == null && !string.IsNullOrWhiteSpace(team.WorkspaceId))
            {
                workspace = await _unitOfWork.Repository<Workspace>()
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Id == team.WorkspaceId);
            }

            if (workspace == null && !string.IsNullOrWhiteSpace(team.CreatedByManagerId))
            {
                var manager = team.CreatedByManager ?? await _userManager.FindByIdAsync(team.CreatedByManagerId);
                if (manager != null && !string.IsNullOrWhiteSpace(manager.WorkspaceId))
                {
                    workspace = await _unitOfWork.Repository<Workspace>()
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(w => w.Id == manager.WorkspaceId);

                    if (workspace != null)
                    {
                        team.WorkspaceId = workspace.Id;
                        team.Workspace = workspace;
                        _unitOfWork.Repository<Domain.Entities.Team>().Update(team);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }

            var token = GenerateSecureToken();

            var now = DateTime.UtcNow;
            var expiresAt = now.AddDays(3);
            Console.WriteLine($"[Invitation] Now: {now}, ExpiresAt: {expiresAt}");

            var invitation = new CompanyInvitation
            {
                Email = dto.Email,
                TeamId = dto.TeamId,
                SeniorityLevel = dto.SeniorityLevel,
                Role = dto.Role,
                InvitationToken = token,
                SentByHRId = hrId,
                SentAt = now,
                ExpiresAt = expiresAt,
                Status = InvitationStatus.Pending
            };

            await _unitOfWork.Repository<CompanyInvitation>().AddAsync(invitation);
            await _unitOfWork.SaveChangesAsync();

            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var invitationLink = $"{frontendUrl}/register?token={token}&email={dto.Email}&orgCode={workspace?.OrgCode ?? string.Empty}";

            var subject = $"Invitation to join the {team.Name} team at SyncVerse";

            // 👔 الـ Email Body الاحترافي والفورمال الجديد بالكامل
            var emailBody = $@"
            <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #334155; background-color: #ffffff; padding: 32px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                
                <!-- Header -->
                <div style='border-bottom: 2px solid #f1f5f9; padding-bottom: 20px; margin-bottom: 24px;'>
                    <h2 style='margin: 0; color: #0f172a; font-size: 22px; font-weight: 700; letter-spacing: -0.5px;'>Team Invitation</h2>
                    <p style='margin: 4px 0 0; color: #64748b; font-size: 14px;'>SyncVerse Enterprise Platform</p>
                </div>

                <p style='color: #0f172a; font-size: 16px; font-weight: 500; margin-bottom: 16px;'>Dear Candidate,</p>

                <p style='font-size: 15px; color: #334155; line-height: 1.6; margin-bottom: 20px;'>
                    You have been officially invited to join the <strong style='color: #0f172a;'>{team.Name}</strong> team within the <strong>{team.Department}</strong> department at <strong>SyncVerse</strong>. This invitation was initiated by <strong>{hr.FirstName} {hr.LastName}</strong> from Human Resources.
                </p>

                <div style='background-color: #f8fafc; border: 1px solid #ebd9fc; border-left: 4px solid #0f172a; padding: 16px; border-radius: 6px; margin: 20px 0;'>
                    <p style='margin: 0 0 10px; font-weight: 600; color: #0f172a; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>Assignment Details</p>
                    <p style='margin: 0; color: #475569; font-size: 14px; line-height: 1.5;'>
                        <strong>Team:</strong> {team.Name}<br/>
                        <strong>Department:</strong> {team.Department}<br/>
                        <strong>Role:</strong> {dto.Role}<br/>
                        <strong>Seniority Level:</strong> {dto.SeniorityLevel}
                    </p>
                </div>

                <p style='font-size: 14px; color: #0b2545; line-height: 1.6; margin-bottom: 12px;'>
                    Please copy this Organization Code:
                    <strong style='color: #0b2545; font-family: monospace; font-size:15px; margin-left:8px;'>{workspace?.OrgCode ?? "N/A"}</strong>
                    to successfully complete your registration by using the button below.
                </p>

                <!-- Action Button -->
                <div style='text-align: center; margin: 28px 0;'>
                    <a href='{invitationLink}' style='background-color: #0b2545; color: #ffffff; text-decoration: none; padding: 12px 32px; font-weight: 600; font-size: 15px; border-radius: 6px; display: inline-block; transition: background-color 0.2s;'>
                        Complete Registration
                    </a>
                </div>

                <!-- Security Notice -->
                <div style='background-color: #fffbeb; border-left: 4px solid #d97706; padding: 14px; border-radius: 4px; margin: 24px 0;'>
                    <p style='margin: 0; font-size: 13px; color: #b45309; line-height: 1.5;'>
                        <strong>Important Security Notice:</strong> This secure registration link contains unique credentials intended strictly for the designated recipient. For security purposes, do not forward this email or share the parameters of this link with anyone else.
                    </p>
                </div>

                <p style='color: #94a3b8; font-size: 12px; text-align: center; margin-top: 24px;'>
                    This corporate invitation is valid until <strong>{invitation.ExpiresAt.ToString("MMMM dd, yyyy")}</strong>.
                </p>

                <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 24px 0 16px;' />

                <p style='color: #94a3b8; font-size: 12px; text-align: center; line-height: 1.5; margin: 0;'>
                    © 2026 SyncVerse Platform. All rights reserved.<br/>
                    This is an automated operational email. Please do not reply directly to this address.
                </p>
            </div>";

            try
            {
                await _emailService.SendAsync(dto.Email, subject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Invitation] Email sending failed for {dto.Email}: {ex.Message}");
                return Result<SendCompanyInvitationResponseDto>.Failure("Company invitation saved, but email sending failed. Please verify SMTP configuration.");
            }

            return Result<SendCompanyInvitationResponseDto>.Success(new SendCompanyInvitationResponseDto
            {
                InvitationToken = token,
                InvitationLink = invitationLink,
                TeamId = dto.TeamId,
                WorkspaceId = workspace?.Id,
                OrgCode = workspace?.OrgCode,
                Email = dto.Email
            }, "Company invitation sent successfully");
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

            return Result<InvitationDetailsDto>.Success(new InvitationDetailsDto
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
                WorkspaceId = invitation.Team.WorkspaceId,
                OrgCode = invitation.Team.Workspace?.OrgCode,
                ExpiresAt = invitation.ExpiresAt,
                IsValid = true
            });
        }

        public async Task<Result<AuthResponseDto>> CompleteProfileAsync(CompleteProfileDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result<AuthResponseDto>.Failure("User not found");

            var invitation = await _unitOfWork.Repository<CompanyInvitation>()
                .Query()
                .Include(i => i.Team)
                .ThenInclude(t => t.Workspace)
                .FirstOrDefaultAsync(i => i.InvitationToken == dto.Token);

            if (invitation == null) return Result<AuthResponseDto>.Failure("Invalid invitation token");
            if (invitation.Status != InvitationStatus.Pending) return Result<AuthResponseDto>.Failure("Invitation already used or expired");

            if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Address)) user.Address = dto.Address;
            if (dto.Skills != null && dto.Skills.Any()) user.Skills = dto.Skills;
            if (dto.Gender != null) user.Gender = dto.Gender;

            if (dto.ProfilePicture != null)
            {
                var fileExtension = Path.GetExtension(dto.ProfilePicture.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                using var stream = dto.ProfilePicture.OpenReadStream();
                var filePath = await _fileStorageService.UploadFileAsync(stream, fileName, "profile-pictures");

                user.ProfilePictureUrl = filePath;
            }

            user.SeniorityLevel = invitation.SeniorityLevel;
            user.Department = invitation.Team.Department;

            var workspace = invitation.Team.Workspace;
            if (workspace == null && !string.IsNullOrWhiteSpace(invitation.Team.WorkspaceId))
            {
                workspace = await _unitOfWork.Repository<Workspace>()
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Id == invitation.Team.WorkspaceId);
            }

            if (workspace == null)
            {
                var hrUser = await _userManager.FindByIdAsync(invitation.SentByHRId);
                if (hrUser != null && !string.IsNullOrWhiteSpace(hrUser.WorkspaceId))
                {
                    workspace = await _unitOfWork.Repository<Workspace>()
                        .Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(w => w.Id == hrUser.WorkspaceId);
                }
            }

            if (workspace != null && string.IsNullOrWhiteSpace(user.WorkspaceId))
            {
                user.WorkspaceId = workspace.Id;
                user.Workspace = workspace;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return Result<AuthResponseDto>.Failure("Failed to update user profile");

            await _userManager.AddToRoleAsync(user, invitation.Role.ToString());
            await ReplaceUserClaimAsync(user, "Department", user.Department.ToString());
            await ReplaceUserClaimAsync(user, "SeniorityLevel", user.SeniorityLevel.ToString());

            var existingTeamMember = await _unitOfWork.Repository<TeamMember>()
                .Query()
                .FirstOrDefaultAsync(tm => tm.TeamId == invitation.TeamId && tm.UserId == user.Id);

            var memberRole = invitation.Role == ProjectRole.TeamLeader
                ? ProjectRole.TeamLeader
                : ProjectRole.TeamMember;

            if (existingTeamMember == null)
            {
                await _unitOfWork.Repository<TeamMember>().AddAsync(new TeamMember
                {
                    TeamId = invitation.TeamId,
                    UserId = user.Id,
                    Role = memberRole,
                    IsActive = true
                });
            }
            else
            {
                existingTeamMember.Role = memberRole;
                existingTeamMember.IsActive = true;
                _unitOfWork.Repository<TeamMember>().Update(existingTeamMember);
            }

            if (invitation.Role == ProjectRole.TeamLeader && invitation.Team != null)
            {
                invitation.Team.TeamLeaderId = user.Id;
                _unitOfWork.Repository<Domain.Entities.Team>().Update(invitation.Team);
            }

            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            Console.WriteLine($"[Invitation] Token: {invitation.InvitationToken} status changed to Accepted at {invitation.AcceptedAt}");
            await _unitOfWork.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var tokenInfo = _jwtHandler.GenerateToken(user, roles, claims);

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
                    PhoneNumber = user.PhoneNumber,
                    Skills = user.Skills,
                    Address = user.Address,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Department = user.Department,
                    SeniorityLevel = user.SeniorityLevel,
                    Roles = roles.ToList(),
                    WorkspaceId = user.WorkspaceId,
                    OrgCode = workspace?.OrgCode,
                    Gender = user.Gender
                },
                Message = "Profile completed and team membership assigned successfully"
            });
        }

        private string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

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