using GuestFlow.Application.Operations.Email;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Operations.Payment.Dtos;
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
    /// Payment reminder background service - sends payment reminders for overdue invoices
    /// </summary>
    public class PaymentReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentReminderBackgroundService> _logger;

        public PaymentReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<PaymentReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaymentReminderBackgroundService başlatıldı.");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    try
                    {
                        await SendPaymentRemindersAsync(scope.ServiceProvider);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Ödeme hatırlatmaları gönderilirken hata: {ex.Message}");
                    }
                }

                // Her gün saat 09:00'da çalıştır
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1).AddHours(9); // Tomorrow at 9 AM
                var delay = nextRun - now;
                _logger.LogInformation($"Bir sonraki çalıştırma: {nextRun:yyyy-MM-dd HH:mm:ss}");
                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task SendPaymentRemindersAsync(IServiceProvider serviceProvider)
        {
            var invoiceRepository = serviceProvider.GetRequiredService<IRepository<InvoicesEntity>>();
            var paymentStatusService = serviceProvider.GetRequiredService<IPaymentStatusService>();
            var emailService = serviceProvider.GetRequiredService<IEmailService>();
            var notificationRepository = serviceProvider.GetRequiredService<IRepository<NotificationEntity>>();

            // Get overdue invoices (due date passed and not fully paid)
            var overdueInvoices = await invoiceRepository.GetAll()
                .Include(i => i.Guest)
                .Where(i => !i.IsDeleted &&
                           i.Status == InvoiceStatus.Generated && // Only generated invoices
                           i.IssueDate.AddDays(30) < DateTime.UtcNow) // Overdue by 30+ days
                .ToListAsync();

            foreach (var invoice in overdueInvoices)
            {
                // Check payment status
                var paymentStatus = await paymentStatusService.GetInvoicePaymentStatusAsync(invoice.Id);

                if (paymentStatus.PaymentStatus == "Paid")
                    continue; // Already paid

                // Check if reminder already sent today
                var todayReminder = await notificationRepository.GetAll()
                    .FirstOrDefaultAsync(n =>
                        n.RelatedEntityType == "Invoice" &&
                        n.RelatedEntityId == invoice.Id &&
                        n.NotificationType == "PaymentReminder" &&
                        n.CreatedDate.Date == DateTime.UtcNow.Date);

                if (todayReminder != null)
                    continue; // Already sent reminder today

                await SendPaymentReminderAsync(invoice, paymentStatus, emailService, notificationRepository);
            }
        }

        private async Task SendPaymentReminderAsync(
            InvoicesEntity invoice,
            InvoicePaymentStatusDto paymentStatus,
            IEmailService emailService,
            IRepository<NotificationEntity> notificationRepository)
        {
            if (invoice.Guest?.Email == null) return;

            var overdueDays = (DateTime.UtcNow.Date - invoice.IssueDate.Date).Days;
            var subject = $"Ödeme Hatırlatması - Fatura #{invoice.InvoiceNumber}";
            var body = GeneratePaymentReminderEmail(invoice, paymentStatus, overdueDays);

            await emailService.SendEmailAsync(invoice.Guest.Email, subject, body);

            // Log notification
            var notification = new NotificationEntity
            {
                Title = subject,
                Content = $"{overdueDays} gün gecikmiş ödeme hatırlatması gönderildi",
                NotificationType = "Email",
                RecipientEmail = invoice.Guest.Email,
                RecipientGuestId = invoice.GuestId,
                Status = "Sent",
                SentDate = DateTime.UtcNow,
                RelatedEntityType = "Invoice",
                RelatedEntityId = invoice.Id
            };

            await notificationRepository.AddAsync(notification);
            _logger.LogInformation($"Ödeme hatırlatması gönderildi - Fatura: {invoice.InvoiceNumber}, Gecikme: {overdueDays} gün");
        }

        private string GeneratePaymentReminderEmail(InvoicesEntity invoice, InvoicePaymentStatusDto paymentStatus, int overdueDays)
        {
            var urgencyLevel = overdueDays > 60 ? "YÜKSEK" : overdueDays > 30 ? "ORTA" : "DÜŞÜK";

            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #d32f2f;'>Ödeme Hatırlatması</h2>
                    <p>Değerli {invoice.Guest?.FullName},</p>

                    <div style='background-color: #fff3e0; padding: 15px; border-left: 4px solid #ff9800; margin: 20px 0;'>
                        <strong>ÖNEMLİ:</strong> Faturanız {overdueDays} gündür ödenmemiştir.
                        <br><strong>Acil durum seviyesi:</strong> {urgencyLevel}
                    </div>

                    <h3>Fatura Detayları:</h3>
                    <ul>
                        <li><strong>Fatura No:</strong> {invoice.InvoiceNumber}</li>
                        <li><strong>Düzenleme Tarihi:</strong> {invoice.IssueDate:dd/MM/yyyy}</li>
                        <li><strong>Toplam Tutar:</strong> {invoice.TotalAmount} {invoice.Currency}</li>
                        <li><strong>Ödenen Tutar:</strong> {paymentStatus.PaidAmount} {invoice.Currency}</li>
                        <li><strong>Kalan Tutar:</strong> {paymentStatus.RemainingAmount} {invoice.Currency}</li>
                        <li><strong>Ödeme Durumu:</strong> {paymentStatus.PaymentStatus}</li>
                    </ul>

                    <div style='background-color: #e3f2fd; padding: 15px; margin: 20px 0;'>
                        <h4>Ödeme Seçenekleri:</h4>
                        <ul>
                            <li>Kredi Kartı / Banka Kartı</li>
                            <li>Banka Havalesi</li>
                            <li>Otel Resepsiyonunda Nakit</li>
                        </ul>
                    </div>

                    <p style='color: #d32f2f; font-weight: bold;'>
                        Lütfen en kısa sürede ödemenizi tamamlayın. Hizmet kalitemizi sürdürebilmek için düzenli ödemeler önemlidir.
                    </p>

                    <p>Ödeme ile ilgili sorularınız için concierge ekibimizle iletişime geçebilirsiniz.</p>

                    <p>Saygılarımla,<br>
                    Finans Departmanı<br>
                    {DateTime.UtcNow:dd/MM/yyyy}</p>
                </div>
            ";
        }
    }
}
