using System;
namespace GuestFlow.Application.Models.Requests.Supplier
{
    public class CreateSupplierCostRequest
    {
        public int SupplierId { get; set; }
        public int? TransferId { get; set; }
        public int? CityTourId { get; set; }
        public int? YachtTourId { get; set; }
        public decimal CostAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string? CostType { get; set; }
        public string? Description { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}

