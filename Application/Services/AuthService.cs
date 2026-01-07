using AutoMapper;
using Graduation_Project.API.JwtFeatuers;
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

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            var user = _mapper.Map<User>(dto);
            user.UserName = dto.Email;

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Message = "Invalid Email or Password", 
                    Errors = new List<string> { "Authentication failed due to incorrect credentials." }
                };
            }

            var token = _jwtHandler.CreateToken(user);

            return new AuthResponseDto
            {
                IsAuthenticated = true,
                Token = token
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Invalid email or password" }
                };
            }

            var token = _jwtHandler.CreateToken(user);

            return new AuthResponseDto
            {
                IsAuthenticated = true,
                Token = token
            };
        }
    }
}
