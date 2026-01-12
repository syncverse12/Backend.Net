using AutoMapper;
using Graduation_Project.API.JwtFeatuers;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Auth;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Graduation_Project.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtHandler _jwtHandler;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<User> userManager,
            JwtHandler jwtHandler,
            IMapper mapper)
        {
            _userManager = userManager;
            _jwtHandler = jwtHandler;
            _mapper = mapper;
        }

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
        {
            var user = _mapper.Map<User>(dto);
            user.UserName = dto.Email;

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return Result<AuthResponseDto>.Failure(
                    "Registration Failed",
                    result.Errors.Select(e => e.Description).ToList()
                );
            }

            await _userManager.AddToRoleAsync(user, "Employee");

            var token = await _jwtHandler.CreateTokenAsync(user);

            return Result<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    Token = token,
                    IsAuthenticated = true 
                },
                "User Registered Successfully"
            );
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Result<AuthResponseDto>.Failure(
                    "Login Failed",
                    new List<string> { "Invalid email or password" }
                );
            }

            var token = await _jwtHandler.CreateTokenAsync(user);

            return Result<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    Token = token,
                    IsAuthenticated = true 
                },
                "Login Successful"
            );
        }
    }
}