using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Models.Responses.Privacy;

namespace GuestFlow.Application.Operations.Privacy
{
    /// <summary>
    /// PII (Personally Identifiable Information) Management Service
    /// Provides data masking and anonymization for KVKK/GDPR compliance
    /// </summary>
    public interface IPIIManagementService
    {
        /// <summary>
        /// Mask PII data for display (partial masking)
        /// </summary>
        string MaskEmail(string email);
        string MaskPhone(string phone);
        string MaskIdentityNumber(string identityNumber);
        string MaskPassportNumber(string passportNumber);
        string MaskAddress(string address);
        string MaskCreditCard(string cardNumber);

        /// <summary>
        /// Anonymize guest data (full anonymization for GDPR right to be forgotten)
        /// </summary>
        Task<bool> AnonymizeGuestAsync(int guestId, string reason, int? requestedByPersonnelId);

        /// <summary>
        /// Delete guest data completely (hard delete - use with caution)
        /// </summary>
        Task<bool> DeleteGuestDataAsync(int guestId, string reason, int? requestedByPersonnelId);

        /// <summary>
        /// Get anonymization/deletion history
        /// </summary>
        Task<List<PrivacyActionHistoryDto>> GetPrivacyActionHistoryAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? guestId = null);

        /// <summary>
        /// Check if guest data is anonymized
        /// </summary>
        Task<bool> IsGuestAnonymizedAsync(int guestId);
    }
}
