using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Auth
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRepository<RefreshTokenEntity> _refreshTokenRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly int _refreshTokenExpireDays;

        public RefreshTokenService(
            IRepository<RefreshTokenEntity> refreshTokenRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<RefreshTokenService> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _personnelRepository = personnelRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            _refreshTokenExpireDays = int.Parse(_configuration["Jwt:RefreshTokenExpireDays"] ?? "30");
        }

        public async Task<string> GenerateRefreshTokenAsync(int personnelId, string? ipAddress = null)
        {
            try
            {
                // Güvenli rastgele token oluştur
                var randomBytes = new byte[64];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }
                var token = Convert.ToBase64String(randomBytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");

                var refreshToken = new RefreshTokenEntity
                {
                    Token = token,
                    PersonnelId = personnelId,
                    ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpireDays),
                    IsRevoked = false,
                    CreatedByIp = ipAddress,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Refresh token oluşturuldu: PersonnelId: {personnelId}");
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Refresh token oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            try
            {
                var tokenEntity = await _refreshTokenRepository.GetAll()
                    .Include(rt => rt.Personnel)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && !rt.IsDeleted);

                if (tokenEntity == null)
                {
                    return new RefreshTokenResult
                    {
                        IsSuccess = false,
                        Message = "Geçersiz refresh token."
                    };
                }

                if (tokenEntity.ExpiresAt < DateTime.UtcNow)
                {
                    return new RefreshTokenResult
                    {
                        IsSuccess = false,
                        Message = "Refresh token'ın süresi dolmuş."
                    };
                }

                var personnel = tokenEntity.Personnel;
                if (personnel == null)
                {
                    return new RefreshTokenResult
                    {
                        IsSuccess = false,
                        Message = "Kullanıcı bulunamadı."
                    };
                }

                // Yeni access token oluştur
                var accessToken = GenerateAccessToken(personnel);

                // Eski refresh token'ı iptal et
                tokenEntity.IsRevoked = true;
                tokenEntity.RevokedAt = DateTime.UtcNow;
                tokenEntity.RevokedByIp = ipAddress;
                await _refreshTokenRepository.UpdateAsync(tokenEntity);

                // Yeni refresh token oluştur
                var newRefreshToken = await GenerateRefreshTokenAsync(personnel.Id, ipAddress);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Token yenilendi: PersonnelId: {personnel.Id}");

                return new RefreshTokenResult
                {
                    IsSuccess = true,
                    Message = "Token başarıyla yenilendi.",
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Token yenilenirken hata: {ex.Message}");
                return new RefreshTokenResult
                {
                    IsSuccess = false,
                    Message = $"Token yenilenirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null)
        {
            try
            {
                var tokenEntity = await _refreshTokenRepository.GetAll()
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && !rt.IsDeleted);

                if (tokenEntity == null)
                {
                    return false;
                }

                tokenEntity.IsRevoked = true;
                tokenEntity.RevokedAt = DateTime.UtcNow;
                tokenEntity.RevokedByIp = ipAddress;
                await _refreshTokenRepository.UpdateAsync(tokenEntity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Refresh token iptal edildi: PersonnelId: {tokenEntity.PersonnelId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Refresh token iptal edilirken hata: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RevokeAllTokensAsync(int personnelId, string? ipAddress = null)
        {
            try
            {
                var tokens = await _refreshTokenRepository.GetAll()
                    .Where(rt => rt.PersonnelId == personnelId && !rt.IsRevoked && !rt.IsDeleted)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = ipAddress;
                    await _refreshTokenRepository.UpdateAsync(token);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Tüm refresh token'lar iptal edildi: PersonnelId: {personnelId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Refresh token'lar iptal edilirken hata: {ex.Message}");
                return false;
            }
        }

        public async Task<int> CleanExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _refreshTokenRepository.GetAll()
                    .Where(rt => rt.ExpiresAt < DateTime.UtcNow && !rt.IsDeleted)
                    .ToListAsync();

                foreach (var token in expiredTokens)
                {
                    await _refreshTokenRepository.DeleteAsync(token.Id);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Süresi dolmuş {expiredTokens.Count} token temizlendi.");
                return expiredTokens.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Süresi dolmuş token'lar temizlenirken hata: {ex.Message}");
                return 0;
            }
        }

        private string GenerateAccessToken(PersonnelEntity personnel)
        {
            var tokenGenerator = new JwtTokenGenerator(_configuration);
            return tokenGenerator.GenerateAccessToken(personnel);
        }
    }
}

