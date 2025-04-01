using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyRevenue
{
    public class DailyRevenueBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyRevenueBackgroundService> _logger;

        public DailyRevenueBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<DailyRevenueBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyRevenueBackgroundService başlatıldı.");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dailyRevenueJob = scope.ServiceProvider.GetRequiredService<DailyRevenueJob>();
                    await dailyRevenueJob.CalculateDailyRevenue(DateTime.UtcNow.Date);
                }

                // Bir sonraki gün gece yarısına kadar bekle
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1);
                var delay = nextRun - now;
                _logger.LogInformation("Bir sonraki çalıştırma", nextRun);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}