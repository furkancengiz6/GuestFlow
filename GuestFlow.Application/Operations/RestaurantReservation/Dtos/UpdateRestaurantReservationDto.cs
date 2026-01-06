using GuestFlow.Domain.Entities.Enum;
using System;

namespace GuestFlow.Application.Operations.RestaurantReservation.Dtos
{
    public class UpdateRestaurantReservationDto
    {
        public int Id { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int NumberOfGuests { get; set; }
        public string? TableNumber { get; set; }
        public string? SpecialRequests { get; set; }
        public ReservationStatus Status { get; set; }
        public int? TransferId { get; set; }
        public int? ReturnTransferId { get; set; }
        public string? Notes { get; set; }
    }
}

