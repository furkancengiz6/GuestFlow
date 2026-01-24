using System.Collections.Generic;

namespace GuestFlow.Application.Models.Responses.Accounting
{
    public class JournalLineDto
    {
        public string AccountCode { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Description { get; set; }
        public string Currency { get; set; } = "USD"; // Line currency
        public decimal? ExchangeRate { get; set; } // Exchange rate to journal currency
    }

    public class JournalPreviewResponse
    {
        public int InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public List<JournalLineDto> Lines { get; set; } = new();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }

    public class JournalEntryResponse
    {
        public int JournalEntryId { get; set; }
        public int InvoiceId { get; set; }
        public string PostingDate { get; set; } = string.Empty; // ISO date string for UI
        public string Description { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD"; // Journal entry base currency
        public List<JournalLineDto> Lines { get; set; } = new();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public string? CreatedBy { get; set; }
        public string? PostedBy { get; set; } // User who posted the journal
        public string? PostedDate { get; set; } // ISO date string for UI
    }

    public class JournalPostRequest
    {
        public int InvoiceId { get; set; }
        public string PostingDate { get; set; } = string.Empty;
        public List<JournalLineDto> Lines { get; set; } = new();
    }

    public class ReverseJournalRequest
    {
        public string? Description { get; set; } // Optional custom description for reversal entry
    }
}

