using GuestFlow.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Transfer.Dtos
{
    public class GetTransferDto
    {
        public int Id { get; set; }
        public DateTime TransferDate { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string DropoffAddress { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public int GuestId { get; set; }
        public int? PersonnelId { get; set; }
        public int? AirportId { get; set; }
        public int? VehicleId { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public bool IsFromAirport { get; set; }
        public TransferType TransferType { get; set; } = TransferType.Custom; // Transfer tipi
        public int? PickupCityId { get; set; }
        public int? HotelId { get; set; } // Otel ID
        public int? RestaurantId { get; set; } // Restoran ID
        public int? DropoffCityId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Ödeme bilgileri
        public PaymentMethod? PaymentMethod { get; set; }
        public bool IsPaymentReceived { get; set; }
        public string? PaymentNote { get; set; }

        // Payment status (calculated from PaymentEntity)
        public string? PaymentStatus { get; set; } // Unpaid, PartiallyPaid, Paid
        public decimal? PaidAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public Dictionary<string, decimal>? PaidAmountByCurrency { get; set; }
        public Dictionary<string, decimal>? RemainingAmountByCurrency { get; set; }
        
        // Şoför bilgileri
        public string? DriverName { get; set; }
        
        // Dışarıdan çekilen araç ve şoför bilgileri
        public string? ExternalVehiclePlate { get; set; }
        public string? ExternalDriverName { get; set; }
        public string? ExternalDriverPhone { get; set; }

        // Priority and transport fields
        public GuestFlow.Domain.Entities.Enum.TransferPriority Priority { get; set; }
        public GuestFlow.Domain.Entities.Enum.TransportMode? TransportMode { get; set; }
        public int? LuggageCount { get; set; }
        public bool IsVip { get; set; }
    }
}
