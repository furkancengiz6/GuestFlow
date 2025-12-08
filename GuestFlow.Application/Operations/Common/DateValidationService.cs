using System;

namespace GuestFlow.Application.Operations.Common
{
    /// <summary>
    /// Tarih validasyon servisi - Tekrarlanan tarih validasyon mantığını merkezileştirir
    /// </summary>
    public interface IDateValidationService
    {
        /// <summary>
        /// Tarihin geçmişte olup olmadığını kontrol eder
        /// </summary>
        bool IsPastDate(DateTime date);

        /// <summary>
        /// Tarihin geçmişte olup olmadığını kontrol eder ve hata mesajı döndürür
        /// </summary>
        (bool IsValid, string? ErrorMessage) ValidateNotPastDate(DateTime date, string entityName = "Tarih");
    }

    public class DateValidationService : IDateValidationService
    {
        public bool IsPastDate(DateTime date)
        {
            return date.Date < DateTime.UtcNow.Date;
        }

        public (bool IsValid, string? ErrorMessage) ValidateNotPastDate(DateTime date, string entityName = "Tarih")
        {
            if (IsPastDate(date))
            {
                return (false, $"{entityName} bugünden önceki bir tarih olamaz.");
            }
            return (true, null);
        }
    }
}

