using System;
using System.Collections.Generic;

namespace GuestFlow.Domain.Entities.Core
{
    public class JournalEntry : BaseEntity
    {
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

