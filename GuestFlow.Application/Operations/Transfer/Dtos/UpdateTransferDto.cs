using GuestFlow.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Transfer.Dtos
{
    public class UpdateTransferDto
    {
        public int Id { get; set; }
        public DateTime TransferDate { get; set; }
        public TimeSpan? PickupTime { get; set; } // Pickup time (different from service start)
        public TimeSpan? ServiceStartTime { get; set; } // When actual transport service begins
        public DateTime? PickupConfirmationTime { get; set; } // When driver confirms pickup
        public DateTime? DropoffConfirmationTime { get; set; } // When driver confirms dropoff
        public string PickupAddress { get; set; } = string.Empty;
        public string DropoffAddress { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int GuestId { get; set; }
        public int? PersonnelId { get; set; } // Otomatik doldurulacak, nullable
        public int? DriverId { get; set; } // Assigned driver (from Personnel)
        public int? AirportId { get; set; } // Zorunlu değil
        public int? VehicleId { get; set; } // Zorunlu değil
        public string? Note { get; set; }
        public string? Status { get; set; }
        public TransferType TransferType { get; set; } = TransferType.Custom; // Transfer tipi
        public int? PickupCityId { get; set; } // Zorunlu değil
        public int? DropoffCityId { get; set; } // Zorunlu değil
        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; } // Para birimi (TRY, USD, EUR, vb.)

        // Guest coordination fields
        public string? ContactPersonName { get; set; } // Who to ask for at pickup
        public string? MeetingPointDetails { get; set; } // Specific pickup instructions

        // Group management fields
        public int? GroupSize { get; set; } // Total number of people
        public int? ChildCount { get; set; } // Number of children
        public int? InfantCount { get; set; } // Number of infants

        // Communication fields
        public string? GuestLanguage { get; set; } // Communication language
        public string? EmergencyContactPhone { get; set; } // Emergency contact phone
        public string? PrimaryContactPhone { get; set; } // Main contact phone (concierge)
        public string? SecondaryContactPhone { get; set; } // Secondary contact phone (group transfers)

        // Driver information
        public string? DriverName { get; set; } // Şoför isim soyisim

        // Dışarıdan çekilen araç ve şoför bilgileri
        public string? ExternalVehiclePlate { get; set; } // Dışarıdan çekilen araç plakası
        public string? ExternalDriverName { get; set; } // Dışarıdan çekilen şoför isim soyisim
        public string? ExternalDriverPhone { get; set; } // Dışarıdan çekilen şoför telefon numarası

        // Supplier cost information (for internal tracking only)
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        public string? SupplierCurrency { get; set; }
        public string? SupplierInvoiceNumber { get; set; }
        public string? SupplierContactPhone { get; set; } // Primary supplier contact
        public string? SupplierEmergencyContact { get; set; } // Emergency supplier contact

        // Service quality fields
        public string? AccessibilityRequirements { get; set; } // Wheelchair, walking assistance, etc.
        public string? SpecialHandlingNotes { get; set; } // VIP handling, medical conditions, etc.

        // Internal coordination fields
        public string? ConciergeInternalNotes { get; set; } // For concierge staff only
        public string? GuestVisibleNotes { get; set; } // Information sent to guest

        // Priority and transport fields
        public GuestFlow.Domain.Entities.Enum.TransferPriority Priority { get; set; } = GuestFlow.Domain.Entities.Enum.TransferPriority.Normal;
        public GuestFlow.Domain.Entities.Enum.TransportMode? TransportMode { get; set; }
        public int? LuggageCount { get; set; }
        public bool IsVip { get; set; } = false;
    }
}
