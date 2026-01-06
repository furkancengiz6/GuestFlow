using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.YachtTour.Dtos
{
    public class AddYachtTourDto
    {
        public DateTime TourDate { get; set; }
        public int NumberOfPeople { get; set; }
        public int? ChildCount { get; set; } // Number of children
        public int? InfantCount { get; set; } // Number of infants
        public decimal Price { get; set; }
        public string? SpecialRequest { get; set; }
        public string? YachtName { get; set; }

        // Group coordination fields
        public string? GroupLeaderName { get; set; } // Responsible person for the group
        public string? GroupLeaderPhone { get; set; } // Group leader contact phone
        public string? EmergencyContactName { get; set; } // Secondary emergency contact
        public string? EmergencyContactPhone { get; set; } // Secondary emergency phone
        public string? EmergencyContactRelation { get; set; } // Relationship to emergency contact

        public int OwnerGuestId { get; set; }
        public int? PersonnelId { get; set; } // Otomatik doldurulacak, nullable
        public int CityId { get; set; }
        /// <summary>
        /// INVOICE REALITY: Invoices are NOT created automatically.
        /// Default is FALSE - invoice creation is time-based (checkout, end-of-day, manual).
        /// </summary>
        public bool CreateInvoice { get; set; } = false;
        public decimal? DiscountPercentage { get; set; }
        public string? InvoiceDescription { get; set; }
        public string? Currency { get; set; } // Para birimi (TRY, USD, EUR, vb.)

        // İskele bilgileri
        public string? PickupPier { get; set; } // Alış iskelesi
        public string? DropoffPier { get; set; } // Bırakış iskelesi
        public string? PierAddress { get; set; } // Serbest metin

        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; } // Başlangıç saati
        public TimeSpan? EndTime { get; set; } // Bitiş saati
        public DateTime? SafetyBriefingTime { get; set; } // When safety briefing occurs
        public TimeSpan? MarinaPickupTime { get; set; } // Marina pickup time
        public DateTime? WeatherCheckTime { get; set; } // When weather is checked
        public DateTime? FuelLevelCheck { get; set; } // When fuel was last checked

        // Yacht and captain information
        public int? YachtId { get; set; } // Assigned yacht (from Yacht entity)
        public int? CaptainId { get; set; } // Assigned captain (from Personnel)
        public GuestFlow.Domain.Entities.Enum.TourCategory? TourCategory { get; set; }
        public bool? LifeGuardCertified { get; set; } // Captain has lifeguard certification
        public DateTime? CoastGuardInspectionDate { get; set; } // Last coast guard inspection

        // Guest safety fields
        public string? SwimmingProficiency { get; set; } // All can swim, supervision needed
        public string? MedicalConditions { get; set; } // Motion sickness, heart conditions
        public string? AlcoholPolicy { get; set; } // Can guests drink? Guidelines

        // Amenities & experience fields
        public bool? FoodBeverageIncluded { get; set; } // Guest expectation management
        public string? BeverageType { get; set; } // Soft drinks, local wines, full bar
        public bool? MusicSystem { get; set; } // Sound system available
        public string? WaterSportsEquipment { get; set; } // Snorkels, paddleboards, etc.

        // Coordination fields
        public string? MarinaContactName { get; set; } // Who to contact at marina
        public string? MarinaContactPhone { get; set; } // Direct marina contact

        // Safety & regulatory fields
        public bool? LifeJacketsProvided { get; set; } // Legal requirement
        public int? LifeJacketCount { get; set; } // Match to group size
        public bool? SafetyEquipmentCheck { get; set; } // Captain confirmed all equipment
        public string? EmergencyEquipment { get; set; } // First aid, VHF radio, etc.

        // Capacity & compliance fields
        public int? YachtCapacity { get; set; } // Legal maximum passengers
        public string? YachtType { get; set; } // Motor Yacht, Sailing Yacht, Catamaran
        public bool? YachtLicenceRequired { get; set; } // Special license needed?
        public bool? CoastGuardApproved { get; set; } // Regulatory compliance

        // Operational details
        public int? CrewSize { get; set; } // Service quality indicator
        public string? CaptainExperience { get; set; } // Years at sea, certifications
        public int? FuelRange { get; set; } // How far yacht can safely travel
        public string? WeatherBackupPlan { get; set; } // What happens if weather deteriorates

        // Supplier cost information (for internal tracking only)
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        public string? SupplierCurrency { get; set; }
        public string? SupplierInvoiceNumber { get; set; }

        // Internal coordination fields
        public string? ConciergeInternalNotes { get; set; } // For concierge staff only
    }
}
