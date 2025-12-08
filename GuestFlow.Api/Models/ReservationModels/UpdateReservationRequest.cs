using System;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.ReservationModels
{
    /// <summary>
    /// Rezervasyon güncelleme request modeli
    /// </summary>
    public class UpdateReservationRequest
    {
        [MaxLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
        public string? Notes { get; set; }

        public DateTime? ReservationDate { get; set; }
    }
}

