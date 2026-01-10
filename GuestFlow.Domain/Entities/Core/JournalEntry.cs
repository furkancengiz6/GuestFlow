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
        public string Currency { get; set; } = "USD";
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
    }
}

