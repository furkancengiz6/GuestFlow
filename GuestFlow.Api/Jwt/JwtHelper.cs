using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GuestFlow.Api.Jwt
{
    public static class JwtHelper
    {
        // Bu metodumla bir JWT token üretiyorum.
        public static string GenerateJwtToken(JwtDto jwtInfo)
        {
            // Önce, gizli anahtarı (secret key) oluşturuyorum. Bunu UTF-8 formatında bir byte dizisine çeviriyorum.
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtInfo.SecretKey));
            // Bu anahtarı kullanarak imzalama bilgilerini (credentials) oluşturuyorum. HMAC-SHA256 algoritmasını kullanıyorum.
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            // Token'a ekleyeceğim bilgileri (claims) hazırlıyorum.
            // Bu bilgiler, token'ın içinde taşınacak ve kullanıcıyı tanımlayacak.
            var claims = new[]
            {
                new Claim("id", jwtInfo.Id.ToString()), // Kullanıcının ID'sini ekliyorum.
                new Claim("FullName", jwtInfo.FullName), // Kullanıcının tam adını ekliyorum.
                new Claim("Email", jwtInfo.Email), // Kullanıcının e-posta adresini ekliyorum.
                new Claim("UserType", jwtInfo.UserType.ToString()), // Kullanıcının tipini ekliyorum.
                new Claim(ClaimTypes.Role, jwtInfo.UserType.ToString()) // Kullanıcının rolünü ekliyorum, bu yetkilendirme için kullanılabilir.
            };

            // Token'ın geçerlilik süresini belirliyorum. Şu anki zamana, verilen dakika süresini ekliyorum.
            var expireTime = DateTime.UtcNow.AddMinutes(jwtInfo.ExpireMinutes);
            // Token'ı oluşturuyorum. Issuer, audience, claims, geçerlilik süresi ve imzalama bilgilerini ekliyorum.
            var tokenDescriptor = new JwtSecurityToken(jwtInfo.Issuer, jwtInfo.Audience, claims, null, expireTime, credentials);
            // Token'ı string formatına çeviriyorum.
            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            // Oluşturduğum token'ı geri döndürüyorum.
            return token;
        }
    }
}