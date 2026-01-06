using GuestFlow.Domain.Entities.Enum;
using System;

namespace GuestFlow.Application.Operations.RestaurantReservation.Dtos
{
    public class GetRestaurantReservationDto
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public int PersonnelId { get; set; }
        public string PersonnelName { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int NumberOfGuests { get; set; }
        public string? TableNumber { get; set; }
        public string? SpecialRequests { get; set; }
        public ReservationStatus Status { get; set; }
        public string? ConfirmationNumber { get; set; }
        public int? TransferId { get; set; }
        public int? ReturnTransferId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

