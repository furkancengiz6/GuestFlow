using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GuestFlow.Api.Jwt
{
    public static class JwtHelper
    {
        public static string GenerateJwtToken(JwtDto jwtInfo)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtInfo.SecretKey));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", jwtInfo.Id.ToString()), // JwtClaimNames.Id yerine doğrudan "id" kullanıldı
                new Claim("FullName", jwtInfo.FullName), // JwtClaimNames.FullName yerine doğrudan "FullName" kullanıldı
                new Claim("Email", jwtInfo.Email), // JwtClaimNames.Email yerine doğrudan "Email" kullanıldı
                new Claim("UserType", jwtInfo.UserType.ToString()), // JwtClaimNames.UserType yerine doğrudan "UserType" kullanıldı
                new Claim(ClaimTypes.Role, jwtInfo.UserType.ToString()) // Role claim'i
            };

            var expireTime = DateTime.UtcNow.AddMinutes(jwtInfo.ExpireMinutes);
            var tokenDescriptor = new JwtSecurityToken(jwtInfo.Issuer, jwtInfo.Audience, claims, null, expireTime, credentials);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return token;
        }
    }
}