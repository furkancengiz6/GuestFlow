namespace GuestFlow.Api.Models
{
    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResponse
    {
        public string Message { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}
