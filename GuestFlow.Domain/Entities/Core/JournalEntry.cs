using System;
using System.Collections.Generic;

namespace GuestFlow.Domain.Entities.Core
{
    public class JournalEntry : BaseEntity
    {
        // Idempotency anchor: each invoice can be posted at most once.
        // Nullable for backward compatibility with existing rows.
        public int? InvoiceId { get; set; }

        public DateTime PostingDate { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD"; // Journal Entry base currency
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        // Audit fields: Hybrid approach (ID + Snapshot)
        // ID for referential integrity and joins, Snapshot for historical accuracy
        public string? CreatedBy { get; set; } // Snapshot: User FullName at creation time (for historical accuracy)
        public int? CreatedByPersonnelId { get; set; } // Foreign key: Personnel ID (for joins and referential integrity)
        public string? PostedBy { get; set; } // Snapshot: User FullName at posting time (for historical accuracy)
        public int? PostedByPersonnelId { get; set; } // Foreign key: Personnel ID who posted (for joins and referential integrity)
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? PostedDate { get; set; } // When the journal was posted

        // Reversal tracking (unpost is NOT allowed - only reversal entries)
        public bool IsReversed { get; set; } = false; // Whether this entry has been reversed
        public int? ReversedByJournalEntryId { get; set; } // ID of the reversal entry that reversed this entry
        public string? ReversedBy { get; set; } // Snapshot: User FullName at reversal time (for historical accuracy)
        public int? ReversedByPersonnelId { get; set; } // Foreign key: Personnel ID who created reversal (for joins and referential integrity)
        public DateTime? ReversedDate { get; set; } // When the reversal entry was created

        // Navigation properties
        public virtual PersonnelEntity? CreatedByPersonnel { get; set; }
        public virtual PersonnelEntity? PostedByPersonnel { get; set; }
        public virtual PersonnelEntity? ReversedByPersonnel { get; set; }

        public virtual ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
    }
}

