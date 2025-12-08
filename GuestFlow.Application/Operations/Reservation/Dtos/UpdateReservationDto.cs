using System;

namespace GuestFlow.Application.Operations.Reservation.Dtos
{
    /// <summary>
    /// Rezervasyon güncelleme DTO'su
    /// </summary>
    public class UpdateReservationDto
    {
        public int Id { get; set; }
        public string? Notes { get; set; }
        public DateTime? ReservationDate { get; set; }
    }
}

