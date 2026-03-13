using SyncVerse.Application.DTOs.Auth;
using SyncVerse.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SyncVerse.API.JwtFeatuers
{
    public class JwtHandler
    {
        private readonly IConfiguration _configuration;

        public JwtHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime Expiration) GenerateToken(User user, IList<string> roles, IEnumerable<Claim>? userClaims = null)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["securityKey"]!));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (userClaims != null)
            {
                claims.AddRange(userClaims);
            }

            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
            var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["validIssuer"],
                audience: jwtSettings["validAudience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }
    }
}