using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.CityTour.Dtos
{
   public class AddCityTourDto
    {
        public DateTime TourDate { get; set; }
        public string Language { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public decimal Price { get; set; }

        // Group composition fields
        public int? AdultCount { get; set; } // Number of adults
        public int? ChildCount { get; set; } // Number of children
        public int? InfantCount { get; set; } // Number of infants

        public int OwnerGuestId { get; set; }
        public int? PersonnelId { get; set; } // Otomatik doldurulacak, nullable
        public int CityId { get; set; }
        public int TourId { get; set; } // Admin tarafından eklenen tur
        /// <summary>
        /// INVOICE REALITY: Invoices are NOT created automatically.
        /// Default is FALSE - invoice creation is time-based (checkout, end-of-day, manual).
        /// </summary>
        public bool CreateInvoice { get; set; } = false;
        public decimal? DiscountPercentage { get; set; }
        public string? InvoiceDescription { get; set; }
        public string? Currency { get; set; } // Para birimi (TRY, USD, EUR, vb.)

        // Time fields
        public TimeSpan? StartTime { get; set; } // Başlangıç saati
        public TimeSpan? EndTime { get; set; } // Bitiş saati
        public TimeSpan? PickupTime { get; set; } // Pickup time for tour
        public DateTime? TourConfirmationTime { get; set; } // When tour is confirmed

        // Safety & emergency fields
        public string? GroupLeaderName { get; set; } // Responsible person for the group
        public string? GroupLeaderPhone { get; set; } // Group leader contact phone
        public string? EmergencyContactName { get; set; } // Secondary emergency contact
        public string? EmergencyContactPhone { get; set; } // Secondary emergency phone
        public string? EmergencyContactRelation { get; set; } // Relationship to emergency contact

        // Coordination fields
        public string? MeetingPersonName { get; set; } // Who to meet at pickup point
        public string? MeetingPointDetails { get; set; } // Detailed pickup instructions

        // Vehicle and driver information
        public int? VehicleId { get; set; } // Zorunlu değil
        public int? TourGuideId { get; set; } // Assigned tour guide (from Personnel)
        public int? AssistantGuideId { get; set; } // Assistant guide (from Personnel)
        public string? DriverName { get; set; } // Şoför isim soyisim

        // Guide information
        public string? GuideName { get; set; } // Rehber isim
        public string? GuidePhone { get; set; } // Rehber telefon numarası
        public string? GuideLanguages { get; set; } // Languages guide can speak
        public string? BackupGuideName { get; set; } // Backup guide if primary unavailable
        public string? BackupGuidePhone { get; set; } // Backup guide contact

        // Operational details
        public string? TourDifficultyLevel { get; set; } // Easy, Moderate, Challenging
        public bool? WeatherDependent { get; set; } // Can tour run in bad weather?
        public int? MinimumParticipantCount { get; set; } // Minimum participants required
        public int? MaximumParticipantCount { get; set; } // Maximum participants allowed

        // Guest experience fields
        public string? DietaryRequirements { get; set; } // Food allergies, preferences
        public string? AccessibilityNeeds { get; set; } // Mobility assistance, etc.
        public bool? PhotographyAllowed { get; set; } // Can participants take photos?
        public string? SpecialEquipment { get; set; } // Special equipment needed

        // Dışarıdan çekilen araç ve şoför bilgileri
        public string? ExternalVehiclePlate { get; set; } // Dışarıdan çekilen araç plakası
        public string? ExternalDriverName { get; set; } // Dışarıdan çekilen şoför isim soyisim
        public string? ExternalDriverPhone { get; set; } // Dışarıdan çekilen şoför telefon numarası

        // Supplier cost information (for internal tracking only)
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        public string? SupplierCurrency { get; set; }
        public string? SupplierInvoiceNumber { get; set; }

        // Internal coordination fields
        public string? ConciergeInternalNotes { get; set; } // For concierge staff only
    }
}
