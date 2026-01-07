using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Graduation_Project.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace Graduation_Project.API.JwtFeatuers
{
    public class JwtHandler
    {
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection jwtSettings;

        public JwtHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
            jwtSettings = configuration.GetSection("JwtSettings");
        }

        public string CreateToken(User user)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = GetClaims(user);
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(jwtSettings["securityKey"]);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        private List<Claim> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName)
            };
            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
                issuer : jwtSettings["validIssuer"],
                audience : jwtSettings["validAudience"],
                claims : claims,
                expires : DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expiryInMinutes"])),
                signingCredentials : signingCredentials
                );
            return tokenOptions;
        }
    }
}
