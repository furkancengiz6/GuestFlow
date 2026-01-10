using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Application.Models.Requests.OTA
{
    public class CreateOTAIntegrationRequest
    {
        [Required]
        [StringLength(100)]
        public string ProviderName { get; set; }

        [Required]
        [StringLength(10)]
        public string ProviderCode { get; set; }

        [Required]
        [Url]
        public string ApiEndpoint { get; set; }

        [Required]
        [StringLength(500)]
        public string ApiKey { get; set; }

        [StringLength(500)]
        public string? ApiSecret { get; set; }

        [Url]
        public string? WebhookUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PriceUpdateRequest
    {
        [Required]
        public string RoomTypeId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        public bool IsAvailable { get; set; } = true;
    }

    public class OTAReservationRequest
    {
        [Required]
        public int OTAIntegrationId { get; set; }

        [Required]
        [StringLength(100)]
        public string OTAReservationId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int GuestCount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; }

        [Required]
        [StringLength(200)]
        public string GuestName { get; set; }

        [EmailAddress]
        [StringLength(254)]
        public string? GuestEmail { get; set; }

        [StringLength(20)]
        public string? GuestPhone { get; set; }
    }
}