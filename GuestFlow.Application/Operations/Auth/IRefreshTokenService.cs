using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Auth
{
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Yeni bir refresh token oluşturur
        /// </summary>
        Task<string> GenerateRefreshTokenAsync(int personnelId, string? ipAddress = null);

        /// <summary>
        /// Refresh token'ı doğrular ve yeni access token döndürür
        /// </summary>
        Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null);

        /// <summary>
        /// Refresh token'ı iptal eder (revoke)
        /// </summary>
        Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);

        /// <summary>
        /// Kullanıcının tüm refresh token'larını iptal eder
        /// </summary>
        Task<bool> RevokeAllTokensAsync(int personnelId, string? ipAddress = null);

        /// <summary>
        /// Süresi dolmuş token'ları temizler
        /// </summary>
        Task<int> CleanExpiredTokensAsync();
    }

    public class RefreshTokenResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}

