using SyncVerse.API.JwtFeatuers;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Auth;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.Services.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtHandler _jwtHandler;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            JwtHandler jwtHandler,
            IOtpService otpService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHandler = jwtHandler;
            _otpService = otpService;
            _emailService = emailService;
        }

        public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return Result<RegisterResponseDto>.Failure("Email already registered");

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Result<RegisterResponseDto>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            var defaultRole = "Employee";
            await _userManager.AddToRoleAsync(user, defaultRole);

            var otp = _otpService.GenerateOtp();
            user.OtpCodeHash = _otpService.HashOtp(otp);
            user.OtpExpirationDate = DateTime.UtcNow.AddMinutes(10);
            user.OtpFailedAttempts = 0;

            await _userManager.UpdateAsync(user);

            var emailBody = $@"
                <!DOCTYPE html>
                <html><body><h2>🎉 Welcome!</h2><p>Your OTP Code: <b>{otp}</b></p></body></html>";

            await _emailService.SendAsync(user.Email!, "Verify Your Email", emailBody);

            return Result<RegisterResponseDto>.Success(new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                Message = "Registration successful. Please verify your email.",
                OtpExpiresAt = user.OtpExpirationDate.Value
            });
        }

        public async Task<Result<AuthResponseDto>> VerifyEmailAsync(VerifyEmailDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsEmailVerified) return Result<AuthResponseDto>.Failure("Invalid operation");

            if (!_otpService.VerifyOtp(dto.Otp, user.OtpCodeHash!))
            {
                // Logic for verification failure
                return Result<AuthResponseDto>.Failure("Invalid OTP.");
            }

            user.IsEmailVerified = true;
            user.EmailConfirmed = true;
            user.OtpCodeHash = null;
            user.OtpExpirationDate = null;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtHandler.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = new UserResponseDto { Id = user.Id, Email = user.Email!, FirstName = user.FirstName, LastName = user.LastName, Roles = roles.ToList() },
                Message = "Email verified successfully"
            });
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsEmailVerified)
                return Result<AuthResponseDto>.Failure("Email not verified or invalid credentials.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return Result<AuthResponseDto>.Failure("Invalid email or password");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtHandler.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = new UserResponseDto { Id = user.Id, Email = user.Email!, FirstName = user.FirstName, LastName = user.LastName, Roles = roles.ToList() },
                Message = "Login successful"
            });
        }

        // Keep your existing Forgot Password logic untouched here...
        public async Task<Result<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordDto dto) { throw new NotImplementedException("Add your existing code here"); }
        public async Task<Result<bool>> VerifyForgotPasswordOtpAsync(VerifyOtpDto dto, string userId) { throw new NotImplementedException("Add your code here"); }
        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto dto, string userId) { throw new NotImplementedException("Add your code here"); }
        public async Task<Result<string>> ResendVerificationOtpAsync(string email) { throw new NotImplementedException("Add your code here"); }
    }
}