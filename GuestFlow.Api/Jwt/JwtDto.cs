using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Api.Jwt
{
    public class JwtDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public int TenantId { get; set; }
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpireMinutes { get; set; }
    }
}
