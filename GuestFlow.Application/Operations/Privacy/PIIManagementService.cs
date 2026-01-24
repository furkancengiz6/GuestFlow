using GuestFlow.Application.Models.Responses.Privacy;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Privacy
{
    /// <summary>
    /// PII Management Service implementation
    /// Provides data masking and anonymization for KVKK/GDPR compliance
    /// </summary>
    public class PIIManagementService : IPIIManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PIIManagementService> _logger;

        public PIIManagementService(
            IUnitOfWork unitOfWork,
            ILogger<PIIManagementService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;

            var parts = email.Split('@');
            if (parts.Length != 2)
                return email; // Invalid email format

            var username = parts[0];
            var domain = parts[1];

            // Mask username: show first 2 chars, mask the rest
            var maskedUsername = username.Length > 2
                ? username.Substring(0, 2) + new string('*', Math.Min(username.Length - 2, 4)) + "@"
                : new string('*', username.Length) + "@";

            return maskedUsername + domain;
        }

        public string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            // Remove non-digit characters
            var digits = Regex.Replace(phone, @"[^\d]", "");
            
            if (digits.Length <= 4)
                return new string('*', digits.Length);

            // Show last 4 digits, mask the rest
            var masked = new string('*', digits.Length - 4) + digits.Substring(digits.Length - 4);
            
            // Preserve original format if possible
            if (phone.Contains("-") || phone.Contains(" "))
            {
                // Try to maintain format
                return masked;
            }

            return masked;
        }

        public string MaskIdentityNumber(string identityNumber)
        {
            if (string.IsNullOrEmpty(identityNumber))
                return string.Empty;

            if (identityNumber.Length <= 4)
                return new string('*', identityNumber.Length);

            // Show first 2 and last 2, mask the middle
            return identityNumber.Substring(0, 2) + 
                   new string('*', identityNumber.Length - 4) + 
                   identityNumber.Substring(identityNumber.Length - 2);
        }

        public string MaskPassportNumber(string passportNumber)
        {
            if (string.IsNullOrEmpty(passportNumber))
                return string.Empty;

            if (passportNumber.Length <= 4)
                return new string('*', passportNumber.Length);

            // Show first 2 and last 2, mask the middle
            return passportNumber.Substring(0, 2) + 
                   new string('*', passportNumber.Length - 4) + 
                   passportNumber.Substring(passportNumber.Length - 2);
        }

        public string MaskAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                return string.Empty;

            // Mask most of the address, show only first few characters
            if (address.Length <= 10)
                return new string('*', address.Length);

            return address.Substring(0, 5) + new string('*', Math.Min(address.Length - 5, 20));
        }

        public string MaskCreditCard(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return string.Empty;

            var digits = Regex.Replace(cardNumber, @"[^\d]", "");
            
            if (digits.Length < 4)
                return new string('*', digits.Length);

            // Show last 4 digits
            return "****-****-****-" + digits.Substring(digits.Length - 4);
        }

        public async Task<bool> AnonymizeGuestAsync(int guestId, string reason, int? requestedByPersonnelId)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                {
                    _logger.LogWarning($"Guest {guestId} not found for anonymization");
                    return false;
                }

                if (guest.IsAnonymized)
                {
                    _logger.LogWarning($"Guest {guestId} is already anonymized");
                    return false;
                }

                // Anonymize PII data
                guest.FullName = $"Anonymized Guest {guestId}";
                guest.Email = $"anonymized.{guestId}@deleted.local";
                guest.PhoneNumber = "***-***-****";
                guest.EmergencyContactName = null;
                guest.EmergencyContactPhone = null;
                guest.IsAnonymized = true;

                // Anonymize related data
                // Note: We keep the relationships but anonymize the data
                // This maintains referential integrity while removing PII

                await _unitOfWork.Guests.UpdateAsync(guest);

                // Record privacy action
                var privacyAction = new PrivacyActionHistoryEntity
                {
                    GuestId = guestId,
                    ActionType = "Anonymize",
                    Reason = reason,
                    RequestedByPersonnelId = requestedByPersonnelId,
                    ActionDate = DateTime.UtcNow
                };

                await _unitOfWork.PrivacyActionHistories.AddAsync(privacyAction);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Guest {guestId} anonymized successfully. Reason: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error anonymizing guest {guestId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteGuestDataAsync(int guestId, string reason, int? requestedByPersonnelId)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                {
                    _logger.LogWarning($"Guest {guestId} not found for deletion");
                    return false;
                }

                // Record privacy action BEFORE deletion
                var privacyAction = new PrivacyActionHistoryEntity
                {
                    GuestId = guestId,
                    ActionType = "Delete",
                    Reason = reason,
                    RequestedByPersonnelId = requestedByPersonnelId,
                    ActionDate = DateTime.UtcNow
                };

                await _unitOfWork.PrivacyActionHistories.AddAsync(privacyAction);

                // Soft delete (mark as deleted)
                // Note: Hard delete would break referential integrity
                // We use soft delete to maintain data integrity while removing access
                guest.IsDeleted = true;
                guest.IsAnonymized = true;
                
                // Also anonymize before soft delete
                guest.FullName = $"Deleted Guest {guestId}";
                guest.Email = $"deleted.{guestId}@deleted.local";
                guest.PhoneNumber = "***-***-****";
                guest.EmergencyContactName = null;
                guest.EmergencyContactPhone = null;

                await _unitOfWork.Guests.UpdateAsync(guest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Guest {guestId} deleted successfully. Reason: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting guest {guestId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<PrivacyActionHistoryDto>> GetPrivacyActionHistoryAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? guestId = null)
        {
            try
            {
                var query = _unitOfWork.PrivacyActionHistories.GetAll();

                if (startDate.HasValue)
                    query = query.Where(p => p.ActionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(p => p.ActionDate <= endDate.Value.AddDays(1).AddTicks(-1));

                if (guestId.HasValue)
                    query = query.Where(p => p.GuestId == guestId.Value);

                var history = await query
                    .Include(p => p.RequestedByPersonnel)
                    .OrderByDescending(p => p.ActionDate)
                    .ToListAsync();

                return history.Select(p => new PrivacyActionHistoryDto
                {
                    Id = p.Id,
                    GuestId = p.GuestId,
                    ActionType = p.ActionType,
                    Reason = p.Reason,
                    RequestedByPersonnelId = p.RequestedByPersonnelId,
                    RequestedByPersonnelName = p.RequestedByPersonnel?.FullName,
                    ActionDate = p.ActionDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting privacy action history");
                throw;
            }
        }

        public async Task<bool> IsGuestAnonymizedAsync(int guestId)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                return guest?.IsAnonymized ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking anonymization status for guest {guestId}");
                return false;
            }
        }
    }
}
