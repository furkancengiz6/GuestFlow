using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.File
{
    /// <summary>
    /// Dosya paylaşım linkleri servisi
    /// </summary>
    public interface IFileShareService
    {
        /// <summary>
        /// Geçici paylaşım linki oluşturur
        /// </summary>
        Task<FileShareLinkDto> CreateShareLinkAsync(string fileName, int expirationHours = 24, string? password = null);

        /// <summary>
        /// Paylaşım linkini doğrular ve dosya bilgisini döndürür
        /// </summary>
        Task<FileShareLinkDto?> ValidateShareLinkAsync(string shareToken);

        /// <summary>
        /// Paylaşım linkini iptal eder
        /// </summary>
        Task<bool> RevokeShareLinkAsync(string shareToken);

        /// <summary>
        /// Aktif paylaşım linklerini getirir
        /// </summary>
        Task<List<FileShareLinkDto>> GetActiveShareLinksAsync(string? fileName = null);
    }

    public class FileShareService : IFileShareService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileShareService> _logger;
        private readonly IFileService _fileService;
        private readonly Dictionary<string, FileShareLinkDto> _shareLinks = new Dictionary<string, FileShareLinkDto>();

        public FileShareService(
            IConfiguration configuration,
            ILogger<FileShareService> logger,
            IFileService fileService)
        {
            _configuration = configuration;
            _logger = logger;
            _fileService = fileService;
        }

        public Task<FileShareLinkDto> CreateShareLinkAsync(string fileName, int expirationHours = 24, string? password = null)
        {
            try
            {
                var shareToken = Guid.NewGuid().ToString("N");
                var baseUrl = _configuration["FileSettings:BaseUrl"] ?? _configuration["EmailSettings:BaseUrl"] ?? "http://localhost:5001";
                var shareUrl = $"{baseUrl}/api/files/share/{shareToken}";

                var shareLink = new FileShareLinkDto
                {
                    ShareToken = shareToken,
                    FileName = fileName,
                    ShareUrl = shareUrl,
                    ExpirationDate = DateTime.UtcNow.AddHours(expirationHours),
                    CreatedDate = DateTime.UtcNow,
                    HasPassword = !string.IsNullOrEmpty(password),
                    Password = password, // Not: Production'da hash'lenmeli
                    IsActive = true
                };

                _shareLinks[shareToken] = shareLink;

                _logger.LogInformation($"Paylaşım linki oluşturuldu: {shareToken}, Dosya: {fileName}, Süre: {expirationHours} saat");

                return Task.FromResult(shareLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Paylaşım linki oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        public Task<FileShareLinkDto?> ValidateShareLinkAsync(string shareToken)
        {
            try
            {
                if (!_shareLinks.TryGetValue(shareToken, out var shareLink))
                {
                    return Task.FromResult<FileShareLinkDto?>(null);
                }

                // Süre kontrolü
                if (shareLink.ExpirationDate < DateTime.UtcNow)
                {
                    _shareLinks.Remove(shareToken);
                    return Task.FromResult<FileShareLinkDto?>(null);
                }

                // Aktiflik kontrolü
                if (!shareLink.IsActive)
                {
                    return Task.FromResult<FileShareLinkDto?>(null);
                }

                // Erişim sayısını artır
                shareLink.AccessCount++;
                shareLink.LastAccessedDate = DateTime.UtcNow;

                return Task.FromResult<FileShareLinkDto?>(shareLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Paylaşım linki doğrulanırken hata: {ex.Message}");
                return Task.FromResult<FileShareLinkDto?>(null);
            }
        }

        public Task<bool> RevokeShareLinkAsync(string shareToken)
        {
            try
            {
                if (_shareLinks.TryGetValue(shareToken, out var shareLink))
                {
                    shareLink.IsActive = false;
                    _logger.LogInformation($"Paylaşım linki iptal edildi: {shareToken}");
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Paylaşım linki iptal edilirken hata: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task<List<FileShareLinkDto>> GetActiveShareLinksAsync(string? fileName = null)
        {
            try
            {
                var activeLinks = _shareLinks.Values
                    .Where(link => link.IsActive && link.ExpirationDate > DateTime.UtcNow)
                    .Where(link => fileName == null || link.FileName == fileName)
                    .ToList();

                return Task.FromResult(activeLinks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Aktif paylaşım linkleri getirilirken hata: {ex.Message}");
                return Task.FromResult(new List<FileShareLinkDto>());
            }
        }
    }

    /// <summary>
    /// Dosya paylaşım linki DTO
    /// </summary>
    public class FileShareLinkDto
    {
        public string ShareToken { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool HasPassword { get; set; }
        public string? Password { get; set; }
        public bool IsActive { get; set; }
        public int AccessCount { get; set; }
        public DateTime? LastAccessedDate { get; set; }
    }
}

