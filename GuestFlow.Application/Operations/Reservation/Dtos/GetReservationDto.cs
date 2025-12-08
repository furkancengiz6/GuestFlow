using System;

namespace GuestFlow.Application.Operations.Reservation.Dtos
{
    /// <summary>
    /// Rezervasyon listesi DTO'su
    /// </summary>
    public class GetReservationDto
    {
        public int Id { get; set; }
        public string ReservationNumber { get; set; }
        public int GuestId { get; set; }
        public string GuestName { get; set; }
        public int PersonnelId { get; set; }
        public string PersonnelName { get; set; }
        public string ServiceType { get; set; }
        public int ServiceId { get; set; }
        public string Status { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

