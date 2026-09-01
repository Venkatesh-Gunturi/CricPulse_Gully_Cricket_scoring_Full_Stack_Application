using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//JWT required system packages
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using CricPulse.Application.Interfaces.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using UserEntity = CricPulse.Domain.Entities.User;

namespace CricPulse.Infrastructure.Authentication
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(UserEntity user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = jwtSettings["key"];
            var issuer = jwtSettings["issuer"];
            var audience = jwtSettings["Audience"];

            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"]!);

            var claims = new List<Claim>
            { 
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName), new Claim(ClaimTypes.MobilePhone, user.MobileNumber)
            };

            if(!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email,user.Email));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer: issuer, audience: audience, claims: claims, 
                             expires: DateTime.UtcNow.AddMinutes(expirationMinutes), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
    }
}
  