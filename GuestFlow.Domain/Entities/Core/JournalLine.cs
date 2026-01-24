using System;

namespace GuestFlow.Domain.Entities.Core
{
    public class JournalLine : BaseEntity
    {
        public int JournalEntryId { get; set; }
        public string AccountCode { get; set; } = string.Empty; // GL code
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Description { get; set; }
        public int? ReferenceId { get; set; } // e.g., InvoiceId
        public string Currency { get; set; } = "USD"; // Line currency (can differ from JournalEntry currency)
        public decimal? ExchangeRate { get; set; } // Exchange rate from line currency to journal currency (if different)

        // Navigation
        public virtual JournalEntry JournalEntry { get; set; } = null!;
    }
}

