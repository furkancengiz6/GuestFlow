using GuestFlow.Domain.Entities.Enum;
using System;

namespace GuestFlow.Application.Operations.RestaurantReservation.Dtos
{
    public class AddRestaurantReservationDto
    {
        public int RestaurantId { get; set; }
        public int GuestId { get; set; }
        public int PersonnelId { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int NumberOfGuests { get; set; }
        public string? TableNumber { get; set; }
        public string? SpecialRequests { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public int? TransferId { get; set; }
        public int? ReturnTransferId { get; set; }
        public string? Notes { get; set; }
        public bool CreateTransfer { get; set; } = false; // Otomatik transfer oluşturulsun mu?
        public bool CreateReturnTransfer { get; set; } = false; // Dönüş transferi oluşturulsun mu?
    }
}

