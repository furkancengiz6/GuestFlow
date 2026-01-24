using System;

namespace GuestFlow.Application.Models.Responses.Privacy
{
    /// <summary>
    /// Privacy action history DTO
    /// </summary>
    public class PrivacyActionHistoryDto
    {
        public int Id { get; set; }
        public int GuestId { get; set; }
        public string ActionType { get; set; } = string.Empty; // "Anonymize" or "Delete"
        public string Reason { get; set; } = string.Empty;
        public int? RequestedByPersonnelId { get; set; }
        public string? RequestedByPersonnelName { get; set; }
        public DateTime ActionDate { get; set; }
    }

    /// <summary>
    /// Anonymize guest request
    /// </summary>
    public class AnonymizeGuestRequest
    {
        public int GuestId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Delete guest request
    /// </summary>
    public class DeleteGuestRequest
    {
        public int GuestId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool ConfirmDeletion { get; set; } // Safety flag
    }
}
