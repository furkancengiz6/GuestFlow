using System;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Privacy action history for audit trail (KVKK/GDPR compliance)
    /// </summary>
    public class PrivacyActionHistoryEntity : BaseEntity
    {
        public int GuestId { get; set; }
        public string ActionType { get; set; } = string.Empty; // "Anonymize" or "Delete"
        public string Reason { get; set; } = string.Empty;
        public int? RequestedByPersonnelId { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual GuestEntity? Guest { get; set; }
        public virtual PersonnelEntity? RequestedByPersonnel { get; set; }
    }
}
