using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _useSsl;
        private readonly string _baseUrl;
        private readonly bool _emailEnabled;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? string.Empty;
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? string.Empty;
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@guestflow.com";
            _fromName = _configuration["EmailSettings:FromName"] ?? "GuestFlow";
            _useSsl = bool.Parse(_configuration["EmailSettings:UseSsl"] ?? "true");
            _baseUrl = _configuration["EmailSettings:BaseUrl"] ?? "http://localhost:5001";
            _emailEnabled = bool.Parse(_configuration["EmailSettings:Enabled"] ?? "false");
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, List<string>? attachments = null)
        {
            try
            {
                // E-posta servisi devre dışıysa
                if (!_emailEnabled)
                {
                    _logger.LogInformation($"E-posta servisi devre dışı. E-posta gönderilmedi: {to}, Konu: {subject}");
                    return false;
                }

                // E-posta ayarları kontrolü
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("E-posta gönderilemedi: SMTP kullanıcı adı veya şifre yapılandırılmamış.");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                // Ekler varsa ekle
                if (attachments != null && attachments.Any())
                {
                    foreach (var attachmentPath in attachments)
                    {
                        if (System.IO.File.Exists(attachmentPath))
                        {
                            bodyBuilder.Attachments.Add(attachmentPath);
                        }
                    }
                }

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpHost, _smtpPort, _useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                    await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"E-posta başarıyla gönderildi: {to}, Konu: {subject}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta gönderilirken hata: {ex.Message}. Alıcı: {to}");
                return false;
            }
        }

        public async Task<bool> SendInvoiceEmailAsync(string to, string guestName, int invoiceNumber, string pdfPath)
        {
            try
            {
                var subject = $"Fatura #{invoiceNumber} - GuestFlow";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4a90e2; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>GuestFlow</h1>
        </div>
        <div class='content'>
            <h2>Sayın {guestName},</h2>
            <p>Faturanız hazırlanmıştır. Fatura detaylarını ekteki PDF dosyasında bulabilirsiniz.</p>
            <p><strong>Fatura Numarası:</strong> {invoiceNumber}</p>
            <p>Faturanızı aşağıdaki linkten de görüntüleyebilirsiniz:</p>
            <p><a href='{_baseUrl}{pdfPath}'>Faturayı Görüntüle</a></p>
            <p>Herhangi bir sorunuz olursa lütfen bizimle iletişime geçin.</p>
            <p>Teşekkürler,<br>GuestFlow Ekibi</p>
        </div>
        <div class='footer'>
            <p>Bu otomatik bir e-postadır. Lütfen yanıtlamayın.</p>
        </div>
    </div>
</body>
</html>";

                var attachments = new List<string>();
                if (System.IO.File.Exists(pdfPath))
                {
                    attachments.Add(pdfPath);
                }

                return await SendEmailAsync(to, subject, body, true, attachments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura e-postası gönderilirken hata: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendBookingConfirmationAsync(string to, string guestName, string bookingType, DateTime bookingDate, string details)
        {
            try
            {
                var subject = $"Rezervasyon Onayı - {bookingType}";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .info-box {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #28a745; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Rezervasyon Onayı</h1>
        </div>
        <div class='content'>
            <h2>Sayın {guestName},</h2>
            <p>Rezervasyonunuz başarıyla oluşturulmuştur.</p>
            <div class='info-box'>
                <p><strong>Rezervasyon Tipi:</strong> {bookingType}</p>
                <p><strong>Tarih:</strong> {bookingDate:dd.MM.yyyy HH:mm}</p>
                <p><strong>Detaylar:</strong></p>
                <p>{details}</p>
            </div>
            <p>Herhangi bir sorunuz olursa lütfen bizimle iletişime geçin.</p>
            <p>Teşekkürler,<br>GuestFlow Ekibi</p>
        </div>
        <div class='footer'>
            <p>Bu otomatik bir e-postadır. Lütfen yanıtlamayın.</p>
        </div>
    </div>
</body>
</html>";

                return await SendEmailAsync(to, subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyon onay e-postası gönderilirken hata: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string to, string fullName, string resetToken)
        {
            try
            {
                var resetUrl = $"{_baseUrl}/api/auth/reset-password?token={resetToken}";
                var subject = "Şifre Sıfırlama - GuestFlow";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .warning {{ background-color: #fff3cd; padding: 15px; margin: 15px 0; border-left: 4px solid #ffc107; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Şifre Sıfırlama</h1>
        </div>
        <div class='content'>
            <h2>Sayın {fullName},</h2>
            <p>Şifre sıfırlama talebiniz alınmıştır. Yeni şifrenizi belirlemek için aşağıdaki butona tıklayın:</p>
            <p style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Şifremi Sıfırla</a>
            </p>
            <p>Veya aşağıdaki linki tarayıcınıza kopyalayıp yapıştırabilirsiniz:</p>
            <p style='word-break: break-all;'>{resetUrl}</p>
            <div class='warning'>
                <p><strong>Önemli:</strong> Bu link 24 saat geçerlidir. Eğer bu talebi siz yapmadıysanız, lütfen bu e-postayı görmezden gelin.</p>
            </div>
            <p>Teşekkürler,<br>GuestFlow Ekibi</p>
        </div>
        <div class='footer'>
            <p>Bu otomatik bir e-postadır. Lütfen yanıtlamayın.</p>
        </div>
    </div>
</body>
</html>";

                return await SendEmailAsync(to, subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şifre sıfırlama e-postası gönderilirken hata: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendDailyRevenueReportAsync(List<string> adminEmails, DateTime date, decimal totalRevenue)
        {
            try
            {
                var subject = $"Günlük Gelir Raporu - {date:dd.MM.yyyy}";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #17a2b8; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .revenue-box {{ background-color: white; padding: 20px; margin: 15px 0; border: 2px solid #17a2b8; text-align: center; }}
        .revenue-amount {{ font-size: 32px; font-weight: bold; color: #17a2b8; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Günlük Gelir Raporu</h1>
        </div>
        <div class='content'>
            <h2>Günlük Gelir Özeti</h2>
            <div class='revenue-box'>
                <p><strong>Tarih:</strong> {date:dd.MM.yyyy}</p>
                <p class='revenue-amount'>{totalRevenue:N2} TRY</p>
            </div>
            <p>Detaylı raporu sistem üzerinden görüntüleyebilirsiniz.</p>
            <p>Teşekkürler,<br>GuestFlow Sistemi</p>
        </div>
        <div class='footer'>
            <p>Bu otomatik bir e-postadır.</p>
        </div>
    </div>
</body>
</html>";

                bool allSent = true;
                foreach (var email in adminEmails)
                {
                    var result = await SendEmailAsync(email, subject, body, true);
                    if (!result)
                        allSent = false;
                }

                return allSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Günlük gelir raporu e-postası gönderilirken hata: {ex.Message}");
                return false;
            }
        }
    }
}
