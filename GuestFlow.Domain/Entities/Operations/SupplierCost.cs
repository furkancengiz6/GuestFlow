using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Domain.Entities.Operations
{
    public class SupplierCost : BaseEntity
    {
        // Foreign Keys
        public int SupplierId { get; set; }
        public int? TransferId { get; set; }
        public int? CityTourId { get; set; }
        public int? YachtTourId { get; set; }
        public int? RestaurantReservationId { get; set; }

        // Cost Details
        public decimal CostAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string? CostType { get; set; } // BaseCost, AdditionalFee, Commission, etc.
        public string? Description { get; set; }

        // Date ranges for seasonal pricing
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        // Status
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Supplier Supplier { get; set; }
        public virtual TransferEntity? Transfer { get; set; }
        public virtual CityTourEntity? CityTour { get; set; }
        public virtual YachtTourEntity? YachtTour { get; set; }
        public virtual RestaurantReservationEntity? RestaurantReservation { get; set; }
    }

    public enum CostType
    {
        BaseCost,
        AdditionalFee,
        Commission,
        FuelCost,
        DriverFee,
        EquipmentCost,
        SeasonalAdjustment,
        Discount
    }
}