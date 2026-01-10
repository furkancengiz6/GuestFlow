using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Application.Models.Requests.Supplier
{
    public class CreateSupplierRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(200)]
        public string? ContactName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Url]
        [StringLength(500)]
        public string? Website { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(3)]
        public string? DefaultCurrency { get; set; } = "USD";

        [Range(0, double.MaxValue)]
        public decimal? DefaultCost { get; set; }
    }
}