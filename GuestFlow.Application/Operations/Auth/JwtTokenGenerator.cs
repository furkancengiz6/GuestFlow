using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GuestFlow.Application.Operations.Auth
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(PersonnelEntity personnel)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", personnel.Id.ToString()),
                new Claim("FullName", personnel.FullName),
                new Claim("Email", personnel.Email),
                new Claim("UserType", personnel.UserType.ToString()),
                new Claim("TenantId", personnel.TenantId.ToString()),
                new Claim(ClaimTypes.Role, personnel.UserType.ToString())
            };

            var expireTime = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"]!));
            var tokenDescriptor = new JwtSecurityToken(
                _configuration["Jwt:Issuer"]!,
                _configuration["Jwt:Audience"]!,
                claims,
                null,
                expireTime,
                credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}

