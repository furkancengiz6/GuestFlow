using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Domain.Entities.Operations
{
    public class OTAIntegration : BaseEntity
    {
        public string ProviderName { get; set; } = string.Empty; // Booking.com, Expedia, etc.
        public string ProviderCode { get; set; } = string.Empty; // BKG, EXP, etc.
        public string ApiEndpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string? ApiSecret { get; set; }
        public string? AccessToken { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? WebhookUrl { get; set; }
        public string? LastSyncStatus { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string? SyncErrorMessage { get; set; }

        // Navigation properties
        // public virtual ICollection<OTAHotelMapping> HotelMappings { get; set; }
        // public virtual ICollection<OTAReservation> Reservations { get; set; }
    }

    public class OTAHotelMapping : BaseEntity
    {
        public int OTAIntegrationId { get; set; }
        public int HotelId { get; set; }
        public string OTARoomTypeId { get; set; } = string.Empty;
        public string OTARoomTypeName { get; set; } = string.Empty;
        public string GuestFlowRoomType { get; set; } = string.Empty;
        public decimal? PriceMultiplier { get; set; } = 1.0m;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual OTAIntegration OTAIntegration { get; set; } = null!;
        public virtual HotelEntity Hotel { get; set; } = null!;
    }

    public class OTAReservation : BaseEntity
    {
        public int OTAIntegrationId { get; set; }
        public string OTAReservationId { get; set; } = string.Empty;
        public string OTAHotelId { get; set; } = string.Empty;
        public string OTARoomTypeId { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int GuestCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }
        public string Status { get; set; } = string.Empty; // Confirmed, Cancelled, Modified
        public DateTime OTACreatedDate { get; set; }
        public DateTime? OTALastModifiedDate { get; set; }

        // GuestFlow mapping
        public int? GuestFlowReservationId { get; set; }

        // Navigation properties
        public virtual OTAIntegration OTAIntegration { get; set; } = null!;
    }

    public class OTAPriceUpdate : BaseEntity
    {
        public int OTAIntegrationId { get; set; }
        public int HotelId { get; set; }
        public string OTARoomTypeId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public string? UpdateStatus { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation properties
        public virtual OTAIntegration OTAIntegration { get; set; } = null!;
        public virtual HotelEntity Hotel { get; set; } = null!;
    }

    public enum OTAProvider
    {
        BookingCom,
        Expedia,
        Agoda,
        Airbnb,
        TripCom,
        HotelsCom
    }

    public enum OTAReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Modified,
        NoShow,
        Completed
    }
}