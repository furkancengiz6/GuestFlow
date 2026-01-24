// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.OTA
{
    /// <summary>
    /// OTA Webhook Retry Background Service - Failed webhook'ları retry eder
    /// </summary>
    public class OTAWebhookRetryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OTAWebhookRetryBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Her 5 dakikada bir kontrol et

        public OTAWebhookRetryBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<OTAWebhookRetryBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OTA Webhook Retry Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRetryQueueAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing webhook retry queue");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("OTA Webhook Retry Background Service stopped");
        }

        private async Task ProcessRetryQueueAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var otaIntegrationService = scope.ServiceProvider.GetRequiredService<IOTAIntegrationService>();

            try
            {
                // Retry edilecek webhook'ları bul (Failed status, NextRetryAt <= now, retry count < max)
                var now = DateTime.UtcNow;
                var webhooksToRetry = await unitOfWork.OTAWebhookLogs
                    .GetAll(w => w.Status == "Failed" &&
                                !w.IsDeadLetter &&
                                w.RetryCount < w.MaxRetries &&
                                w.NextRetryAt.HasValue &&
                                w.NextRetryAt.Value <= now &&
                                !w.IsDeleted)
                    .Include(w => w.OTAIntegration)
                    .Take(10) // Her seferinde max 10 webhook retry et
                    .ToListAsync(cancellationToken);

                if (webhooksToRetry.Count == 0)
                    return;

                _logger.LogInformation("Processing {Count} webhook retries", webhooksToRetry.Count);

                foreach (var webhookLog in webhooksToRetry)
                {
                    try
                    {
                        if (webhookLog.OTAIntegration == null || !webhookLog.OTAIntegration.IsActive)
                        {
                            _logger.LogWarning("OTA integration not found or inactive for webhook log {Id}", webhookLog.Id);
                            continue;
                        }

                        // Retry webhook processing
                        var result = await otaIntegrationService.ProcessWebhookAsync(
                            webhookLog.ProviderCode,
                            webhookLog.Payload,
                            webhookLog.Signature,
                            webhookLog.IdempotencyKey,
                            webhookLog.IpAddress,
                            webhookLog.UserAgent);

                        if (result.Success)
                        {
                            _logger.LogInformation("Webhook retry successful (Id: {Id}, IdempotencyKey: {Key})", 
                                webhookLog.Id, webhookLog.IdempotencyKey);
                        }
                        else
                        {
                            _logger.LogWarning("Webhook retry failed (Id: {Id}, IdempotencyKey: {Key}): {Error}", 
                                webhookLog.Id, webhookLog.IdempotencyKey, result.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrying webhook (Id: {Id})", webhookLog.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing retry queue");
            }
        }
    }
}
