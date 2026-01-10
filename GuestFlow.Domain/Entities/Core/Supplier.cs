using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Domain.Entities.Core
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; }
        public string Type { get; set; } // Yacht, Transfer, Activity, Restaurant, General
        public string? ContactName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        // Default pricing
        public string? DefaultCurrency { get; set; } = "USD";
        public decimal? DefaultCost { get; set; }

        // Navigation properties - will be added when needed
        // public virtual ICollection<TransferSupplierCost> TransferCosts { get; set; }
        // public virtual ICollection<TourSupplierCost> TourCosts { get; set; }
    }

    public enum SupplierType
    {
        Yacht,
        Transfer,
        Activity,
        Restaurant,
        Hotel,
        General
    }
}