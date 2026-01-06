using System;

namespace GuestFlow.Application.Operations.Common
{
    /// <summary>
    /// Tarih validasyon servisi - Tekrarlanan tarih validasyon mantığını merkezileştirir
    /// 
    /// DATE REALITY (LOCKED PRODUCT DECISION):
    /// - Past-dated entries are ALLOWED and EXPECTED
    /// - Service date represents when the operation ACTUALLY occurred
    /// - The system must NOT block retroactive entries
    /// - ValidateNotPastDate is DEPRECATED and should NOT be used
    /// </summary>
    public interface IDateValidationService
    {
        /// <summary>
        /// Tarihin geçmişte olup olmadığını kontrol eder
        /// </summary>
        bool IsPastDate(DateTime date);

        /// <summary>
        /// DEPRECATED: Past-dated entries are allowed per DATE REALITY.
        /// This method should NOT be used - it's kept only for backward compatibility.
        /// Service date represents when the operation actually occurred, not when entered.
        /// </summary>
        [Obsolete("Past-dated entries are allowed. Do not use this method to block operations.")]
        (bool IsValid, string? ErrorMessage) ValidateNotPastDate(DateTime date, string entityName = "Tarih");
    }

    public class DateValidationService : IDateValidationService
    {
        /// <summary>
        /// Utility method - checks if a date is in the past.
        /// Note: Being a past date is NOT an error - just informational.
        /// </summary>
        public bool IsPastDate(DateTime date)
        {
            return date.Date < DateTime.UtcNow.Date;
        }

        /// <summary>
        /// DEPRECATED: Past-dated entries are allowed per DATE REALITY.
        /// This method now ALWAYS returns (true, null) - past dates are valid.
        /// Service date represents when the operation actually occurred, not when entered.
        /// </summary>
        [Obsolete("Past-dated entries are allowed. Do not use this method to block operations.")]
        public (bool IsValid, string? ErrorMessage) ValidateNotPastDate(DateTime date, string entityName = "Tarih")
        {
            // DATE REALITY: Past dates are ALLOWED
            // Always return valid - do not block retroactive entries
            return (true, null);
        }
    }
}

