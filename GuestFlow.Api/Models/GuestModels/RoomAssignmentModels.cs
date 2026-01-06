using System;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.GuestModels
{
    /// <summary>
    /// Oda ataması oluşturma request modeli
    /// </summary>
    public class CreateRoomAssignmentRequest
    {
        /// <summary>
        /// Misafir ID (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Misafir ID gereklidir.")]
        public int GuestId { get; set; }

        /// <summary>
        /// Otel ID (opsiyonel)
        /// </summary>
        public int? HotelId { get; set; }

        /// <summary>
        /// Oda numarası (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Oda numarası gereklidir.")]
        [StringLength(20, ErrorMessage = "Oda numarası en fazla 20 karakter olabilir.")]
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// Başlangıç tarihi (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Başlangıç tarihi gereklidir.")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi (opsiyonel - null ise devam eden atama)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Notlar (opsiyonel)
        /// </summary>
        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Oda ataması güncelleme request modeli
    /// </summary>
    public class UpdateRoomAssignmentRequest
    {
        /// <summary>
        /// Oda numarası
        /// </summary>
        [Required(ErrorMessage = "Oda numarası gereklidir.")]
        [StringLength(20, ErrorMessage = "Oda numarası en fazla 20 karakter olabilir.")]
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// Başlangıç tarihi
        /// </summary>
        [Required(ErrorMessage = "Başlangıç tarihi gereklidir.")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi (opsiyonel)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Notlar (opsiyonel)
        /// </summary>
        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Oda ataması kapatma request modeli
    /// </summary>
    public class CloseRoomAssignmentRequest
    {
        /// <summary>
        /// Bitiş tarihi (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Bitiş tarihi gereklidir.")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Notlar (opsiyonel)
        /// </summary>
        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Oda bağlamı sorgulama request modeli
    /// </summary>
    public class RoomContextRequest
    {
        /// <summary>
        /// Oda numarası (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Oda numarası gereklidir.")]
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// Başlangıç tarihi (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Başlangıç tarihi gereklidir.")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Bitiş tarihi gereklidir.")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Otel ID (opsiyonel - belirli bir otelde arama için)
        /// </summary>
        public int? HotelId { get; set; }
    }
}