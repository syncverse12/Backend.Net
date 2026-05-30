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

            var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #f4f4f4; background-color: #1a1a1a; padding: 20px; border-radius: 8px;'>
                <div style='text-align: center; border-bottom: 1px solid #444; padding-bottom: 20px; margin-bottom: 20px;'>
                    <h1 style='color: #ffffff; margin: 0;'>Welcome to SyncVerse</h1>
                </div>

                <p style='font-size: 16px; color: #dddddd;'>Dear Candidate,</p>
                
                <p style='font-size: 15px; color: #bbbbbb; line-height: 1.6;'>
                    We are pleased to inform you that you have been invited to join <strong>SyncVerse</strong> as a member of our team. 
                    This invitation has been initiated by <strong>{hr.FirstName} {hr.LastName}</strong> from the Human Resources Department.
                </p>

                <div style='background-color: #252525; padding: 20px; border-radius: 6px; margin: 25px 0;'>
                    <p style='margin-top: 0; font-weight: bold; color: #ffffff; border-bottom: 1px solid #444; padding-bottom: 10px;'>Assignment Details</p>
                    <p style='margin: 10px 0;'>You will be joining the <strong>{team.Name}</strong> team within the <strong>{team.Department}</strong> department. 
                    Your position is designated at the <strong>{dto.SeniorityLevel}</strong> level, where you will be serving in the role of <strong>{dto.Role}</strong>.</p>
                    <p style='margin: 10px 0;'>Organization Code: <strong>{workspace?.OrgCode ?? "N/A"}</strong></p>
                </div>

                <p style='color: #dddddd;'>To complete your registration and access your workspace, please click the button below:</p>

                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{invitationLink}' style='background-color: #314357; color: #ffffff; text-decoration: none; padding: 14px 25px; font-weight: bold; border-radius: 4px; display: inline-block;'>
                        Complete Registration
                    </a>
                </div>

                <div style='background-color: #2c2100; border-left: 4px solid #ffcc00; padding: 15px; margin: 20px 0;'>
                    <p style='margin: 0; font-size: 13px; color: #ffcc00;'>
                        <strong>Security Notice:</strong> This is a personal invitation link intended only for you. 
                        Please do not share this link with others for security reasons.
                    </p>
                </div>

                <p style='font-size: 12px; color: #888; text-align: center;'>
                    This invitation will expire on <strong>{invitation.ExpiresAt.ToString("MMMM dd, yyyy")}</strong>.
                </p>
                
                <hr style='border: none; border-top: 1px solid #333; margin-top: 40px;'/>
                <p style='font-size: 11px; color: #666; text-align: center;'>
                    © 2026 SyncVerse. All rights reserved.<br/>
                    This email was sent from an automated system. Please do not reply.
                </p>
            </div>";

            try
            {
                await _emailService.SendAsync(dto.Email, "Invitation to Join SyncVerse", emailBody);
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

            // ✅ Map Normal Properties
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Address)) user.Address = dto.Address;
            if (dto.Skills != null && dto.Skills.Any()) user.Skills = dto.Skills;
            if (dto.Gender != null) user.Gender = dto.Gender;

            // ✅ Logic to Handle Attachment Upload
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

            if (existingTeamMember == null)
            {
                await _unitOfWork.Repository<TeamMember>().AddAsync(new TeamMember
                {
                    TeamId = invitation.TeamId,
                    UserId = user.Id,
                    Role = ProjectRole.TeamMember,
                    IsActive = true
                });
            }
            else
            {
                existingTeamMember.IsActive = true;
                _unitOfWork.Repository<TeamMember>().Update(existingTeamMember);
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