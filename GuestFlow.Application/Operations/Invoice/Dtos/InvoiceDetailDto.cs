using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Invoice.Dtos
{
    /// <summary>
    /// Fatura detay DTO (ilgili veriler ile)
    /// </summary>
    public class InvoiceDetailDto
    {
        // Temel Bilgiler
        public int Id { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// VAT total snapshot derived from invoice items (VAT-inclusive pricing).
        /// </summary>
        public decimal VatTotal { get; set; }

        /// <summary>
        /// Net amount derived from TotalAmount - VatTotal (VAT-inclusive pricing).
        /// </summary>
        public decimal NetTotal { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string PdfUrl { get; set; } = string.Empty;
        public bool HasPdf => !string.IsNullOrEmpty(PdfUrl);
        public DateTime CreatedDate { get; set; }

        // Payment status (calculated from PaymentEntity)
        public string? PaymentStatus { get; set; } // Unpaid, PartiallyPaid, Paid
        public decimal? PaidAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public Dictionary<string, decimal>? PaidAmountByCurrency { get; set; }
        public Dictionary<string, decimal>? RemainingAmountByCurrency { get; set; }

        // Accounting status (calculated from JournalLines.ReferenceId)
        public bool IsJournalPosted { get; set; }
        public int? JournalEntryId { get; set; }
        public DateTime? JournalPostingDate { get; set; }

        // İlişkili Veriler
        public InvoiceGuestDto? Guest { get; set; }
        public InvoicePersonnelDto? Personnel { get; set; }
        public InvoiceServiceDto? Service { get; set; } // Transfer, CityTour veya YachtTour
    }

    /// <summary>
    /// Fatura misafir DTO
    /// </summary>
    public class InvoiceGuestDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public bool IsSpecialGuest { get; set; }
    }

    /// <summary>
    /// Fatura personel DTO
    /// </summary>
    public class InvoicePersonnelDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string UserType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fatura hizmet DTO (Transfer, CityTour veya YachtTour)
    /// </summary>
    public class InvoiceServiceDto
    {
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public DateTime? ServiceDate { get; set; }
        public decimal? ServiceAmount { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    /// <summary>
    /// Fatura istatistikleri DTO
    /// </summary>
    public class InvoiceStatisticsDto
    {
        public int TotalInvoices { get; set; }
        public int InvoicesWithPdf { get; set; }
        public int InvoicesWithoutPdf { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageInvoiceAmount { get; set; }
        public int TotalGuests { get; set; }
        public Dictionary<string, int> InvoicesByCurrency { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenueByCurrency { get; set; } = new Dictionary<string, decimal>();
    }
}

