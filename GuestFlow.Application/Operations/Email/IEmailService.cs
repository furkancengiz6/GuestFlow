using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Email
{
    public interface IEmailService
    {
        /// <summary>
        /// E-posta gönderir
        /// </summary>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, List<string>? attachments = null);

        /// <summary>
        /// Fatura PDF'i ile e-posta gönderir
        /// </summary>
        Task<bool> SendInvoiceEmailAsync(string to, string guestName, int invoiceNumber, string pdfPath);

        /// <summary>
        /// Rezervasyon onay e-postası gönderir
        /// </summary>
        Task<bool> SendBookingConfirmationAsync(string to, string guestName, string bookingType, DateTime bookingDate, string details);

        /// <summary>
        /// Şifre sıfırlama e-postası gönderir
        /// </summary>
        Task<bool> SendPasswordResetEmailAsync(string to, string fullName, string resetToken);

        /// <summary>
        /// Günlük gelir raporu e-postası gönderir (Admin'lere)
        /// </summary>
        Task<bool> SendDailyRevenueReportAsync(List<string> adminEmails, DateTime date, decimal totalRevenue);
    }
}
