// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.PMS
{
    /// <summary>
    /// PMS Polling Background Service - Aktif PMS entegrasyonları için periyodik senkronizasyon
    /// </summary>
    public class PMSPollingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PMSPollingBackgroundService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromMinutes(5); // Default 5 dakika

        public PMSPollingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<PMSPollingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PMS Polling Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPollingSyncAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in PMS polling background service");
                }

                // Polling interval kadar bekle
                await Task.Delay(_pollingInterval, stoppingToken);
            }

            _logger.LogInformation("PMS Polling Background Service stopped");
        }

        private async Task ProcessPollingSyncAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var pmsSyncService = scope.ServiceProvider.GetRequiredService<IPMSSyncService>();

            try
            {
                // Aktif ve polling mode'da olan entegrasyonları bul
                var activeIntegrations = await unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && 
                                 !i.IsDeleted && 
                                 i.SyncMode == PMSSyncMode.Polling)
                    .ToListAsync(cancellationToken);

                if (!activeIntegrations.Any())
                {
                    _logger.LogDebug("No active PMS integrations found for polling");
                    return;
                }

                _logger.LogInformation("Processing polling sync for {Count} PMS integrations", activeIntegrations.Count);

                foreach (var integration in activeIntegrations)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        // Her entegrasyon için kendi polling interval'ını kullan
                        var pollingInterval = TimeSpan.FromMinutes(integration.PollingIntervalMinutes > 0 
                            ? integration.PollingIntervalMinutes 
                            : 5);

                        // Son sync tarihini kontrol et
                        var lastSync = await unitOfWork.PMSSyncHistories
                            .GetAll(h => h.PMSIntegrationId == integration.Id && 
                                        h.SyncType == PMSSyncType.FullSync &&
                                        h.Status == PMSSyncStatus.Success)
                            .OrderByDescending(h => h.SyncStartTime)
                            .FirstOrDefaultAsync(cancellationToken);

                        // Eğer son sync'ten bu yana yeterli zaman geçmediyse atla
                        if (lastSync != null && 
                            DateTime.UtcNow - lastSync.SyncStartTime < pollingInterval)
                        {
                            _logger.LogDebug("Skipping integration {IntegrationId} - polling interval not reached", integration.Id);
                            continue;
                        }

                        _logger.LogInformation("Starting polling sync for integration {IntegrationId} ({Provider})", 
                            integration.Id, integration.ProviderName);

                        // Son 24 saat için sync yap
                        var startDate = DateTime.UtcNow.AddDays(-1);
                        var endDate = DateTime.UtcNow;

                        // Rezervasyonları senkronize et
                        var reservationsResult = await pmsSyncService.SyncReservationsAsync(
                            integration.Id, startDate, endDate);
                        
                        if (!reservationsResult.Success)
                        {
                            _logger.LogWarning("Failed to sync reservations for integration {IntegrationId}: {Error}", 
                                integration.Id, reservationsResult.Message);
                        }

                        // Misafirleri senkronize et (aktif misafirler)
                        var guestsResult = await pmsSyncService.SyncGuestsAsync(
                            integration.Id, startDate, endDate);
                        
                        if (!guestsResult.Success)
                        {
                            _logger.LogWarning("Failed to sync guests for integration {IntegrationId}: {Error}", 
                                integration.Id, guestsResult.Message);
                        }

                        // Oda durumlarını senkronize et
                        var roomsResult = await pmsSyncService.SyncRoomsStatusAsync(integration.Id, DateTime.UtcNow);
                        
                        if (!roomsResult.Success)
                        {
                            _logger.LogWarning("Failed to sync room statuses for integration {IntegrationId}: {Error}", 
                                integration.Id, roomsResult.Message);
                        }

                        _logger.LogInformation("Completed polling sync for integration {IntegrationId}", integration.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing polling sync for integration {IntegrationId}", integration.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPollingSyncAsync");
            }
        }
    }
}
