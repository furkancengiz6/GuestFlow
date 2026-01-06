using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.CityTour.Dtos
{
    public class UpdateCityTourDto
    {
        public int Id { get; set; }
        public DateTime TourDate { get; set; }
        public string Language { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public decimal Price { get; set; }

        // Group composition fields
        public int? AdultCount { get; set; } // Number of adults
        public int? ChildCount { get; set; } // Number of children
        public int? InfantCount { get; set; } // Number of infants

        public int OwnerGuestId { get; set; }
        public int? PersonnelId { get; set; }
        public int CityId { get; set; }
        public int TourId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; }

        // Time fields
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
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
        public int? VehicleId { get; set; }
        public int? TourGuideId { get; set; } // Assigned tour guide (from Personnel)
        public int? AssistantGuideId { get; set; } // Assistant guide (from Personnel)
        public string? DriverName { get; set; }

        // Guide information
        public string? GuideName { get; set; }
        public string? GuidePhone { get; set; }
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
        public string? ExternalVehiclePlate { get; set; }
        public string? ExternalDriverName { get; set; }
        public string? ExternalDriverPhone { get; set; }

        // Supplier cost information (for internal tracking only)
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        public string? SupplierCurrency { get; set; }
        public string? SupplierInvoiceNumber { get; set; }

        // Internal coordination fields
        public string? ConciergeInternalNotes { get; set; } // For concierge staff only
    }
}
