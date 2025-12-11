using System;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.ReservationModels
{
    /// <summary>
    /// Yeni rezervasyon oluşturma request modeli
    /// </summary>
    public class AddReservationRequest
    {
        [Required(ErrorMessage = "Misafir ID gereklidir.")]
        public int GuestId { get; set; }

        [Required(ErrorMessage = "Personel ID gereklidir.")]
        public int PersonnelId { get; set; }

        [Required(ErrorMessage = "Servis tipi gereklidir.")]
        [RegularExpression("^(Transfer|CityTour|YachtTour)$", ErrorMessage = "Servis tipi Transfer, CityTour veya YachtTour olmalıdır.")]
        public string ServiceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Servis ID gereklidir.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Rezervasyon tarihi gereklidir.")]
        public DateTime ReservationDate { get; set; }

        [MaxLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
        public string? Notes { get; set; }
    }
}

