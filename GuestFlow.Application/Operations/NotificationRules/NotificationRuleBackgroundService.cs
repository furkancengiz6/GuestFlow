// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.NotificationRules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.NotificationRules
{
    /// <summary>
    /// Notification Rules Background Service - Aktif kuralları periyodik olarak çalıştırır
    /// </summary>
    public class NotificationRuleBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationRuleBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // Her 30 dakikada bir kontrol et

        public NotificationRuleBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationRuleBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationRuleBackgroundService başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var ruleService = scope.ServiceProvider.GetRequiredService<INotificationRuleService>();
                        var result = await ruleService.ExecuteAllActiveRulesAsync();

                        if (result.Success && result.Data != null)
                        {
                            var triggeredCount = result.Data.Count(r => r.Triggered);
                            if (triggeredCount > 0)
                            {
                                _logger.LogInformation("Notification rules executed: {TriggeredCount} rules triggered, {TotalNotifications} notifications sent",
                                    triggeredCount, result.Data.Sum(r => r.NotificationsSent));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing notification rules");
                }

                // Bir sonraki kontrol için bekle
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
