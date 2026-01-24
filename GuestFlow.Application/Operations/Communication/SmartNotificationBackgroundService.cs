// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Communication;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Communication
{
    /// <summary>
    /// Smart Notifications Background Service - Otomatik bildirimler gönderir
    /// </summary>
    public class SmartNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SmartNotificationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Her saat kontrol et

        public SmartNotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SmartNotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SmartNotificationBackgroundService başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        await ProcessSmartNotificationsAsync(scope.ServiceProvider);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Smart notification işleme sırasında hata oluştu");
                }

                // Bir sonraki kontrol için bekle
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessSmartNotificationsAsync(IServiceProvider serviceProvider)
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var smartNotificationService = serviceProvider.GetRequiredService<ISmartNotificationService>();
            var now = DateTime.UtcNow;

            // Pre-Arrival Notifications (Check-in'den 1 gün önce)
            await SendPreArrivalNotificationsAsync(unitOfWork, smartNotificationService, now);

            // Arrival Notifications (Check-in günü)
            await SendArrivalNotificationsAsync(unitOfWork, smartNotificationService, now);

            // Pre-Departure Notifications (Check-out'den 1 gün önce)
            await SendPreDepartureNotificationsAsync(unitOfWork, smartNotificationService, now);

            // Special Occasion Notifications (Doğum günü, yıldönümü)
            await SendSpecialOccasionNotificationsAsync(unitOfWork, smartNotificationService, now);

            // During Stay Notifications (Yaklaşan servisler için hatırlatma)
            await SendDuringStayNotificationsAsync(unitOfWork, smartNotificationService, now);
        }

        private async Task SendPreArrivalNotificationsAsync(IUnitOfWork unitOfWork, ISmartNotificationService smartNotificationService, DateTime now)
        {
            try
            {
                // Check-in tarihi bugünden 1 gün sonra olan misafirler
                var tomorrow = now.Date.AddDays(1);
                var guests = await unitOfWork.Guests
                    .GetAll(g => g.CheckInDate.HasValue &&
                                 g.CheckInDate.Value.Date == tomorrow &&
                                 !g.IsDeleted)
                    .ToListAsync();

                foreach (var guest in guests)
                {
                    // Daha önce gönderilmiş mi kontrol et (communication history'den)
                    var emailHistory = await unitOfWork.EmailHistories
                        .GetAll(e => e.To == guest.Email &&
                                     e.TemplateName == "PreArrival" &&
                                     e.SentDate.Date == now.Date)
                        .FirstOrDefaultAsync();

                    if (emailHistory == null)
                    {
                        var result = await smartNotificationService.SendCustomNotificationAsync(guest.Id, "PreArrival", "Welcome! We're looking forward to your arrival.");
                        if (result.Success)
                        {
                            _logger.LogInformation("Pre-Arrival notification sent to guest {GuestId} ({GuestName})", guest.Id, guest.FullName);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to send Pre-Arrival notification to guest {GuestId}: {Error}", guest.Id, result.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pre-Arrival notifications gönderilirken hata oluştu");
            }
        }

        private async Task SendArrivalNotificationsAsync(IUnitOfWork unitOfWork, ISmartNotificationService smartNotificationService, DateTime now)
        {
            try
            {
                // Bugün check-in olan misafirler
                var today = now.Date;
                var guests = await unitOfWork.Guests
                    .GetAll(g => g.CheckInDate.HasValue &&
                                 g.CheckInDate.Value.Date == today &&
                                 !g.IsDeleted)
                    .ToListAsync();

                foreach (var guest in guests)
                {
                    // Daha önce gönderilmiş mi kontrol et
                    var emailHistory = await unitOfWork.EmailHistories
                        .GetAll(e => e.To == guest.Email &&
                                     e.TemplateName == "Arrival" &&
                                     e.SentDate.Date == now.Date)
                        .FirstOrDefaultAsync();

                    if (emailHistory == null)
                    {
                        var result = await smartNotificationService.SendCustomNotificationAsync(guest.Id, "Arrival", "Welcome! We hope you enjoy your stay.");
                        if (result.Success)
                        {
                            _logger.LogInformation("Arrival notification sent to guest {GuestId} ({GuestName})", guest.Id, guest.FullName);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to send Arrival notification to guest {GuestId}: {Error}", guest.Id, result.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Arrival notifications gönderilirken hata oluştu");
            }
        }

        private async Task SendPreDepartureNotificationsAsync(IUnitOfWork unitOfWork, ISmartNotificationService smartNotificationService, DateTime now)
        {
            try
            {
                // Check-out tarihi bugünden 1 gün sonra olan misafirler
                var tomorrow = now.Date.AddDays(1);
                var guests = await unitOfWork.Guests
                    .GetAll(g => g.CheckOutDate.HasValue &&
                                 g.CheckOutDate.Value.Date == tomorrow &&
                                 !g.IsDeleted)
                    .ToListAsync();

                foreach (var guest in guests)
                {
                    // Daha önce gönderilmiş mi kontrol et
                    var emailHistory = await unitOfWork.EmailHistories
                        .GetAll(e => e.To == guest.Email &&
                                     e.TemplateName == "PreDeparture" &&
                                     e.SentDate.Date == now.Date)
                        .FirstOrDefaultAsync();

                    if (emailHistory == null)
                    {
                        var result = await smartNotificationService.SendCustomNotificationAsync(guest.Id, "PreDeparture", "Thank you for staying with us! We hope to see you again soon.");
                        if (result.Success)
                        {
                            _logger.LogInformation("Pre-Departure notification sent to guest {GuestId} ({GuestName})", guest.Id, guest.FullName);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to send Pre-Departure notification to guest {GuestId}: {Error}", guest.Id, result.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pre-Departure notifications gönderilirken hata oluştu");
            }
        }

        private async Task SendSpecialOccasionNotificationsAsync(IUnitOfWork unitOfWork, ISmartNotificationService smartNotificationService, DateTime now)
        {
            try
            {
                // Doğum günü ve yıldönümü kontrolü için PMS verilerinden çekilebilir
                // Doğum günü kontrolü
                var guests = await unitOfWork.Guests
                    .GetAll(g => g.DateOfBirth.HasValue &&
                                 g.DateOfBirth.Value.Month == now.Month &&
                                 g.DateOfBirth.Value.Day == now.Day &&
                                 !g.IsDeleted)
                    .ToListAsync();

                foreach (var guest in guests)
                {
                    // Check if already sent
                    var emailHistory = await unitOfWork.EmailHistories
                        .GetAll(e => e.To == guest.Email &&
                                     e.TemplateName == "Birthday" &&
                                     e.SentDate.Date == now.Date)
                        .FirstOrDefaultAsync();

                    if (emailHistory == null)
                    {
                        var result = await smartNotificationService.SendCustomNotificationAsync(
                            guest.Id, 
                            "Birthday", 
                            "Happy Birthday! We wish you a wonderful year ahead from the GuestFlow team."
                        );

                        if (result.Success)
                        {
                            _logger.LogInformation("Birthday notification sent to guest {GuestId} ({GuestName})", guest.Id, guest.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Special Occasion notifications gönderilirken hata oluştu");
            }
        }

        private async Task SendDuringStayNotificationsAsync(IUnitOfWork unitOfWork, ISmartNotificationService smartNotificationService, DateTime now)
        {
            try
            {
                // Yaklaşan servisler için hatırlatma (24 saat öncesi)
                var tomorrow = now.AddDays(1);

                // Transferler
                var upcomingTransfers = await unitOfWork.Transfers
                    .GetAll(t => t.TransferDate.Date == tomorrow.Date &&
                                 t.Status == "Confirmed" &&
                                 !t.IsDeleted)
                    .Include(t => t.Guest)
                    .ToListAsync();

                foreach (var transfer in upcomingTransfers)
                {
                    if (transfer.Guest != null)
                    {
                        var result = await smartNotificationService.SendCustomNotificationAsync(
                            transfer.Guest.Id,
                            "DuringStay",
                            $"Reminder: Your transfer is scheduled for {transfer.TransferDate:yyyy-MM-dd HH:mm}"
                        );
                        if (result.Success)
                        {
                            _logger.LogInformation("During Stay notification (Transfer) sent to guest {GuestId}", transfer.Guest.Id);
                        }
                    }
                }

                // Şehir turları
                var upcomingCityTours = await unitOfWork.CityTours
                    .GetAll(t => t.TourDate.Date == tomorrow.Date &&
                                 t.Status == "Confirmed" &&
                                 !t.IsDeleted)
                    .ToListAsync();

                // Yat turları
                var upcomingYachtTours = await unitOfWork.YachtTours
                    .GetAll(t => t.TourDate.Date == tomorrow.Date &&
                                 t.Status == "Confirmed" &&
                                 !t.IsDeleted)
                    .ToListAsync();

                // Benzer şekilde diğer servisler için de hatırlatma gönderilebilir
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "During Stay notifications gönderilirken hata oluştu");
            }
        }
    }
}
