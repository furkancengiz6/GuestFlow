using GuestFlow.Application.Operations.Email;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
                    var date = DateTime.UtcNow.Date;
                    await dailyRevenueJob.CalculateDailyRevenue(date);

                    // Günlük gelir raporu e-postası gönder
                    try
                    {
                        var dailyRevenueRepository = scope.ServiceProvider.GetRequiredService<IRepository<DailyRevenueEntity>>();
                        var personnelRepository = scope.ServiceProvider.GetRequiredService<IRepository<PersonnelEntity>>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        var dailyRevenue = await dailyRevenueRepository.GetAsync(dr => dr.Date.Date == date);
                        if (dailyRevenue != null)
                        {
                            // Admin e-postalarını al
                            var adminEmails = await personnelRepository.GetAll()
                                .Where(p => p.UserType == UserType.Admin && !string.IsNullOrEmpty(p.Email))
                                .Select(p => p.Email)
                                .ToListAsync();

                            if (adminEmails.Any())
                            {
                                await emailService.SendDailyRevenueReportAsync(adminEmails, date, dailyRevenue.TotalRevenue);
                                _logger.LogInformation($"Günlük gelir raporu e-postası gönderildi. Tarih: {date:yyyy-MM-dd}, Alıcı sayısı: {adminEmails.Count}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Günlük gelir raporu e-postası gönderilirken hata: {ex.Message}");
                        // E-posta hatası servisi durdurmaz
                    }
                }

                // Bir sonraki gün gece yarısına kadar bekle
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1);
                var delay = nextRun - now;
                _logger.LogInformation($"Bir sonraki çalıştırma: {nextRun:yyyy-MM-dd HH:mm:ss}");
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}