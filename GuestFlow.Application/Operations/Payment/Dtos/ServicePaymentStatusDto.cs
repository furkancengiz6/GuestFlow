using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Payment.Dtos
{
    /// <summary>
    /// Payment status for a specific service (Transfer/CityTour/YachtTour)
    /// </summary>
    public class ServicePaymentStatusDto
    {
        /// <summary>
        /// Service ID
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Service type (Transfer, CityTour, YachtTour)
        /// </summary>
        public string ServiceType { get; set; } = string.Empty;

        /// <summary>
        /// Total service amount
        /// </summary>
        public decimal ServiceAmount { get; set; }

        /// <summary>
        /// Total paid amount from completed payments (legacy - use PaidAmountByCurrency for multi-currency)
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
        /// Guest name for context
        /// </summary>
        public string GuestName { get; set; } = string.Empty;

        /// <summary>
        /// Service date
        /// </summary>
        public DateTime ServiceDate { get; set; }
    }
}
