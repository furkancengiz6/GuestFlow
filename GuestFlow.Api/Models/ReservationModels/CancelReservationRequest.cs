using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.ReservationModels
{
    /// <summary>
    /// Rezervasyon iptal etme request modeli
    /// </summary>
    public class CancelReservationRequest
    {
        [MaxLength(500, ErrorMessage = "İptal nedeni en fazla 500 karakter olabilir.")]
        public string? CancellationReason { get; set; }
    }
}

