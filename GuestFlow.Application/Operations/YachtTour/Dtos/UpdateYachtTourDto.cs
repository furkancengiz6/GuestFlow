using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.YachtTour.Dtos
{
   public class UpdateYachtTourDto
    {
        public int Id { get; set; }
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
        public int? PersonnelId { get; set; }
        public int CityId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; }

        // İskele bilgileri
        public string? PickupPier { get; set; }
        public string? DropoffPier { get; set; }
        public string? PierAddress { get; set; }

        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
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
        public string? SwimmingProficiency { get; set; }
        public string? MedicalConditions { get; set; }
        public string? AlcoholPolicy { get; set; }

        // Amenities & experience fields
        public bool? FoodBeverageIncluded { get; set; }
        public string? BeverageType { get; set; }
        public bool? MusicSystem { get; set; }
        public string? WaterSportsEquipment { get; set; }

        // Coordination fields
        public string? MarinaContactName { get; set; }
        public string? MarinaContactPhone { get; set; }

        // Safety & regulatory fields
        public bool? LifeJacketsProvided { get; set; }
        public int? LifeJacketCount { get; set; }
        public bool? SafetyEquipmentCheck { get; set; }
        public string? EmergencyEquipment { get; set; }

        // Capacity & compliance fields
        public int? YachtCapacity { get; set; }
        public string? YachtType { get; set; }
        public bool? YachtLicenceRequired { get; set; }
        public bool? CoastGuardApproved { get; set; }

        // Operational details
        public int? CrewSize { get; set; }
        public string? CaptainExperience { get; set; }
        public int? FuelRange { get; set; }
        public string? WeatherBackupPlan { get; set; }

        // Supplier cost information (for internal tracking only)
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        public string? SupplierCurrency { get; set; }
        public string? SupplierInvoiceNumber { get; set; }

        // Internal coordination fields
        public string? ConciergeInternalNotes { get; set; }
    }
}
