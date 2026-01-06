using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.InvoiceModels
{
    /// <summary>
    /// Request model for manually creating invoices
    /// </summary>
    public class CreateInvoiceRequest
    {
        [Required(ErrorMessage = "Misafir ID gereklidir.")]
        public int GuestId { get; set; }

        [Required(ErrorMessage = "Para birimi gereklidir.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Para birimi 3 karakter olmalıdır.")]
        public string Currency { get; set; } = "TRY";

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
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
    }

    public class GetEligibleServicesRequest
    {
        [Required(ErrorMessage = "Misafir ID gereklidir.")]
        public int GuestId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
