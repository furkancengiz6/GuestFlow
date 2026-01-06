using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Application.Operations.Invoice.Dtos
{
    /// <summary>
    /// DTO for manually creating invoices.
    /// INVOICE REALITY (LOCKED PRODUCT DECISION):
    /// - Invoices are NOT auto-created on service creation
    /// - Invoice creation is time-based (checkout, end-of-day, manual)
    /// - One invoice may cover multiple services
    /// - Invoices are independent from payments
    /// </summary>
    public class CreateInvoiceDto
    {
        [Required]
        public int GuestId { get; set; }

        [Required]
        public string Currency { get; set; } = "TRY";

        [Required]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Optional date range to filter eligible services
        /// If not provided, uses last 7 days
        /// </summary>
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Optional specific service IDs to include
        /// If not provided, shows eligible services in date range
        /// </summary>
        public List<int>? SelectedServiceIds { get; set; }

        public int? CreatedByPersonnelId { get; set; }
    }

    public class CreateInvoiceItemDto
    {
        [Required]
        public string ServiceType { get; set; } = string.Empty; // "Transfer", "CityTour", "YachtTour"

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "TRY";

        public string? Notes { get; set; }
    }
}
