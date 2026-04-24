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

        public async Task<Result<bool>> SendInvitationAsync(SendCompanyInvitationDto dto, string hrId)
        {

            var hr = await _userManager.FindByIdAsync(hrId);
            if (hr == null) return Result<bool>.Failure("HR user not found");

            var team = await _unitOfWork.Repository<Domain.Entities.Team>().GetByIdAsync(dto.TeamId);
            if (team == null) return Result<bool>.Failure("Team not found");

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
            var invitationLink = $"{frontendUrl}/register?token={token}&email={dto.Email}";

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

            var hrUser = await _userManager.FindByIdAsync(invitation.SentByHRId);
            if (hrUser != null && hrUser.WorkspaceId != null) 
            {
                user.WorkspaceId = hrUser.WorkspaceId;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return Result<AuthResponseDto>.Failure("Failed to update user profile");

            await _userManager.AddToRoleAsync(user, invitation.Role.ToString());
            await ReplaceUserClaimAsync(user, "Department", user.Department.ToString());
            await ReplaceUserClaimAsync(user, "SeniorityLevel", user.SeniorityLevel.ToString());

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
                    Roles = roles.ToList()
                },
                Message = "Profile completed and workspace assigned successfully"
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