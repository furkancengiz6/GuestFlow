using System;

namespace GuestFlow.Application.Operations.Reservation.Dtos
{
    /// <summary>
    /// Yeni rezervasyon oluşturma DTO'su
    /// </summary>
    public class AddReservationDto
    {
        /// <summary>
        /// Misafir ID
        /// </summary>
        public int GuestId { get; set; }

        /// <summary>
        /// Personel ID
        /// </summary>
        public int PersonnelId { get; set; }

        /// <summary>
        /// Rezervasyon tipi (Transfer, CityTour, YachtTour)
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// Servis ID (TransferId, CityTourId veya YachtTourId)
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Rezervasyon tarihi
        /// </summary>
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }
    }
}

