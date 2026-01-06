using GuestFlow.Application.Operations.Email;
using GuestFlow.Domain.Entities.Core;
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
    /// <summary>
    /// Service confirmation background service - sends confirmation reminders 24 hours before service
    /// </summary>
    public class ServiceConfirmationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServiceConfirmationBackgroundService> _logger;

        public ServiceConfirmationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ServiceConfirmationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ServiceConfirmationBackgroundService başlatıldı.");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    try
                    {
                        await SendServiceConfirmationsAsync(scope.ServiceProvider);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Servis onay hatırlatmaları gönderilirken hata: {ex.Message}");
                    }
                }

                // Her saat başı çalıştır
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddHours(now.Hour + 1);
                var delay = nextRun - now;
                _logger.LogInformation($"Bir sonraki çalıştırma: {nextRun:yyyy-MM-dd HH:mm:ss}");
                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task SendServiceConfirmationsAsync(IServiceProvider serviceProvider)
        {
            var transferRepository = serviceProvider.GetRequiredService<IRepository<TransferEntity>>();
            var cityTourRepository = serviceProvider.GetRequiredService<IRepository<CityTourEntity>>();
            var yachtTourRepository = serviceProvider.GetRequiredService<IRepository<YachtTourEntity>>();
            var emailService = serviceProvider.GetRequiredService<IEmailService>();
            var notificationRepository = serviceProvider.GetRequiredService<IRepository<NotificationEntity>>();

            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            var tomorrowEnd = tomorrow.AddDays(1).AddTicks(-1);

            // Transfers for tomorrow
            var transfers = await transferRepository.GetAll()
                .Include(t => t.Guest)
                .Where(t => !t.IsDeleted && t.TransferDate >= tomorrow && t.TransferDate <= tomorrowEnd)
                .ToListAsync();

            foreach (var transfer in transfers)
            {
                await SendTransferConfirmationAsync(transfer, emailService, notificationRepository);
            }

            // City Tours for tomorrow
            var cityTours = await cityTourRepository.GetAll()
                .Include(ct => ct.OwnerGuest)
                .Include(ct => ct.City)
                .Where(ct => !ct.IsDeleted && ct.TourDate >= tomorrow && ct.TourDate <= tomorrowEnd)
                .ToListAsync();

            foreach (var cityTour in cityTours)
            {
                await SendCityTourConfirmationAsync(cityTour, emailService, notificationRepository);
            }

            // Yacht Tours for tomorrow
            var yachtTours = await yachtTourRepository.GetAll()
                .Include(yt => yt.OwnerGuest)
                .Include(yt => yt.City)
                .Where(yt => !yt.IsDeleted && yt.TourDate >= tomorrow && yt.TourDate <= tomorrowEnd)
                .ToListAsync();

            foreach (var yachtTour in yachtTours)
            {
                await SendYachtTourConfirmationAsync(yachtTour, emailService, notificationRepository);
            }
        }

        private async Task SendTransferConfirmationAsync(TransferEntity transfer, IEmailService emailService, IRepository<NotificationEntity> notificationRepository)
        {
            if (transfer.Guest?.Email == null) return;

            // Check if confirmation already sent
            var existingNotification = await notificationRepository.GetAll()
                .FirstOrDefaultAsync(n => n.RelatedEntityType == "Transfer" && n.RelatedEntityId == transfer.Id && n.NotificationType == "Confirmation");

            if (existingNotification != null) return;

            var subject = $"Transfer Onayı - {transfer.TransferDate:dd/MM/yyyy}";
            var body = GenerateTransferConfirmationEmail(transfer);

            await emailService.SendEmailAsync(transfer.Guest.Email, subject, body);

            // Log notification
            var notification = new NotificationEntity
            {
                Title = subject,
                Content = "Transfer onay hatırlatması gönderildi",
                NotificationType = "Email",
                RecipientEmail = transfer.Guest.Email,
                RecipientGuestId = transfer.GuestId,
                Status = "Sent",
                SentDate = DateTime.UtcNow,
                RelatedEntityType = "Transfer",
                RelatedEntityId = transfer.Id
            };

            await notificationRepository.AddAsync(notification);
            _logger.LogInformation($"Transfer onay hatırlatması gönderildi: {transfer.Id}");
        }

        private async Task SendCityTourConfirmationAsync(CityTourEntity cityTour, IEmailService emailService, IRepository<NotificationEntity> notificationRepository)
        {
            if (cityTour.OwnerGuest?.Email == null) return;

            var existingNotification = await notificationRepository.GetAll()
                .FirstOrDefaultAsync(n => n.RelatedEntityType == "CityTour" && n.RelatedEntityId == cityTour.Id && n.NotificationType == "Confirmation");

            if (existingNotification != null) return;

            var subject = $"Şehir Turu Onayı - {cityTour.TourDate:dd/MM/yyyy}";
            var body = GenerateCityTourConfirmationEmail(cityTour);

            await emailService.SendEmailAsync(cityTour.OwnerGuest.Email, subject, body);

            var notification = new NotificationEntity
            {
                Title = subject,
                Content = "Şehir turu onay hatırlatması gönderildi",
                NotificationType = "Email",
                RecipientEmail = cityTour.OwnerGuest.Email,
                RecipientGuestId = cityTour.OwnerGuestId,
                Status = "Sent",
                SentDate = DateTime.UtcNow,
                RelatedEntityType = "CityTour",
                RelatedEntityId = cityTour.Id
            };

            await notificationRepository.AddAsync(notification);
            _logger.LogInformation($"Şehir turu onay hatırlatması gönderildi: {cityTour.Id}");
        }

        private async Task SendYachtTourConfirmationAsync(YachtTourEntity yachtTour, IEmailService emailService, IRepository<NotificationEntity> notificationRepository)
        {
            if (yachtTour.OwnerGuest?.Email == null) return;

            var existingNotification = await notificationRepository.GetAll()
                .FirstOrDefaultAsync(n => n.RelatedEntityType == "YachtTour" && n.RelatedEntityId == yachtTour.Id && n.NotificationType == "Confirmation");

            if (existingNotification != null) return;

            var subject = $"Yat Turu Onayı - {yachtTour.TourDate:dd/MM/yyyy}";
            var body = GenerateYachtTourConfirmationEmail(yachtTour);

            await emailService.SendEmailAsync(yachtTour.OwnerGuest.Email, subject, body);

            var notification = new NotificationEntity
            {
                Title = subject,
                Content = "Yat turu onay hatırlatması gönderildi",
                NotificationType = "Email",
                RecipientEmail = yachtTour.OwnerGuest.Email,
                RecipientGuestId = yachtTour.OwnerGuestId,
                Status = "Sent",
                SentDate = DateTime.UtcNow,
                RelatedEntityType = "YachtTour",
                RelatedEntityId = yachtTour.Id
            };

            await notificationRepository.AddAsync(notification);
            _logger.LogInformation($"Yat turu onay hatırlatması gönderildi: {yachtTour.Id}");
        }

        private string GenerateTransferConfirmationEmail(TransferEntity transfer)
        {
            return $@"
                <h2>Transfer Onayı</h2>
                <p>Değerli {transfer.Guest?.FullName},</p>
                <p>Yarın planlanan transferiniz için onay hatırlatması:</p>
                <ul>
                    <li><strong>Tarih:</strong> {transfer.TransferDate:dd/MM/yyyy}</li>
                    <li><strong>Alış Adresi:</strong> {transfer.PickupAddress}</li>
                    <li><strong>Bırakış Adresi:</strong> {transfer.DropoffAddress}</li>
                    <li><strong>Fiyat:</strong> {transfer.FinalPrice} {transfer.Currency}</li>
                </ul>
                <p>Herhangi bir değişiklik için lütfen bizimle iletişime geçin.</p>
                <p>Saygılarımla,<br>Concierge Ekibi</p>
            ";
        }

        private string GenerateCityTourConfirmationEmail(CityTourEntity cityTour)
        {
            return $@"
                <h2>Şehir Turu Onayı</h2>
                <p>Değerli {cityTour.OwnerGuest?.FullName},</p>
                <p>Yarın planlanan şehir turunuz için onay hatırlatması:</p>
                <ul>
                    <li><strong>Tarih:</strong> {cityTour.TourDate:dd/MM/yyyy}</li>
                    <li><strong>Şehir:</strong> {cityTour.City?.CityName}</li>
                    <li><strong>Dil:</strong> {cityTour.Language}</li>
                    <li><strong>Süre:</strong> {cityTour.DurationHours} saat</li>
                    <li><strong>Fiyat:</strong> {cityTour.FinalPrice} {cityTour.Currency}</li>
                </ul>
                <p>Tur öncesi hazırlıklarınız tamam mı? Herhangi bir değişiklik için lütfen bizimle iletişime geçin.</p>
                <p>Saygılarımla,<br>Concierge Ekibi</p>
            ";
        }

        private string GenerateYachtTourConfirmationEmail(YachtTourEntity yachtTour)
        {
            return $@"
                <h2>Yat Turu Onayı</h2>
                <p>Değerli {yachtTour.OwnerGuest?.FullName},</p>
                <p>Yarın planlanan yat turunuz için onay hatırlatması:</p>
                <ul>
                    <li><strong>Tarih:</strong> {yachtTour.TourDate:dd/MM/yyyy}</li>
                    <li><strong>Şehir:</strong> {yachtTour.City?.CityName}</li>
                    <li><strong>Kişi Sayısı:</strong> {yachtTour.NumberOfPeople}</li>
                    <li><strong>Yat Adı:</strong> {yachtTour.YachtName}</li>
                    <li><strong>Fiyat:</strong> {yachtTour.FinalPrice} {yachtTour.Currency}</li>
                </ul>
                <p>Güvenlik brifingi ve check-in için lütfen zamanında hazır olun. Herhangi bir değişiklik için lütfen bizimle iletişime geçin.</p>
                <p>Saygılarımla,<br>Concierge Ekibi</p>
            ";
        }
    }
}
