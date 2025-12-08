using System;

namespace GuestFlow.Application.Operations.Reservation.Dtos
{
    /// <summary>
    /// Rezervasyon detay DTO'su
    /// </summary>
    public class ReservationDetailDto
    {
        public int Id { get; set; }
        public string ReservationNumber { get; set; }
        public string Status { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedDate { get; set; }

        // Guest Information
        public ReservationGuestDto Guest { get; set; }

        // Personnel Information
        public ReservationPersonnelDto Personnel { get; set; }

        // Service Information (Transfer, CityTour, or YachtTour)
        public string ServiceType { get; set; }
        public int ServiceId { get; set; }
    }

    public class ReservationGuestDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string GuestCode { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Nationality { get; set; }
        public bool IsSpecialGuest { get; set; }
    }

    public class ReservationPersonnelDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string UserType { get; set; }
    }
}

