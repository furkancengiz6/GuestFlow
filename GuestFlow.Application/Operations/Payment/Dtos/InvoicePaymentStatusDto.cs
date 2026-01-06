using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Payment.Dtos
{
    /// <summary>
    /// Payment status for a specific invoice
    /// </summary>
    public class InvoicePaymentStatusDto
    {
        /// <summary>
        /// Invoice ID
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Total invoice amount
        /// </summary>
        public decimal InvoiceAmount { get; set; }

        /// <summary>
        /// Total paid amount from completed payments linked to this invoice (legacy - use PaidAmountByCurrency for multi-currency)
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Paid amount broken down by currency (currency-safe)
        /// </summary>
        public Dictionary<string, decimal> PaidAmountByCurrency { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Remaining amount to be paid (legacy - use RemainingAmountByCurrency for multi-currency)
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Remaining amount broken down by currency (currency-safe)
        /// </summary>
        public Dictionary<string, decimal> RemainingAmountByCurrency { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Currency of the amounts (primary currency for legacy fields)
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Payment status: Unpaid, PartiallyPaid, Paid
        /// </summary>
        public string PaymentStatus { get; set; } = "Unpaid";

        /// <summary>
        /// Number of payments made towards this invoice
        /// </summary>
        public int PaymentCount { get; set; }

        /// <summary>
        /// List of service payment statuses included in this invoice
        /// </summary>
        public List<ServicePaymentStatusDto> ServiceStatuses { get; set; } = new List<ServicePaymentStatusDto>();
    }
}
