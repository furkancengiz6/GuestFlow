using System;

namespace GuestFlow.Application.Operations.Invoice.Dtos
{
    /// <summary>
    /// DTO for services eligible for invoice inclusion
    /// </summary>
    public class EligibleServiceDto
    {
        public string ServiceType { get; set; } = string.Empty; // "Transfer", "CityTour", "YachtTour"
        public int ServiceId { get; set; }
        public string ServiceDescription { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public bool IsAlreadyInvoiced { get; set; }
        public string? GuestName { get; set; }
    }
}
