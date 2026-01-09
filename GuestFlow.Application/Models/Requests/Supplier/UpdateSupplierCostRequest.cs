using System;
namespace GuestFlow.Application.Models.Requests.Supplier
{
    public class UpdateSupplierCostRequest
    {
        public decimal? CostAmount { get; set; }
        public string? Currency { get; set; }
        public string? CostType { get; set; }
        public string? Description { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool? IsActive { get; set; }
    }
}

