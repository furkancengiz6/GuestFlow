using GuestFlow.Application.Operations.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Auth
{
    /// <summary>
    /// Süresi dolmuş refresh token'ları otomatik olarak temizleyen background service
    /// </summary>
    public class RefreshTokenCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RefreshTokenCleanupBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Her 24 saatte bir çalışır

        public RefreshTokenCleanupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<RefreshTokenCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Refresh token temizleme servisi başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
                        
                        var cleanedCount = await refreshTokenService.CleanExpiredTokensAsync();
                        
                        if (cleanedCount > 0)
                        {
                            _logger.LogInformation($"{cleanedCount} süresi dolmuş refresh token temizlendi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Refresh token temizleme sırasında hata oluştu.");
                }

                // Bir sonraki kontrol için bekle
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Refresh token temizleme servisi durduruldu.");
        }
    }
}

