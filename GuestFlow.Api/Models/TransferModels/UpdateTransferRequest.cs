using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.TransferModel
{
    public class UpdateTransferRequest
    {
        [Required]
        public DateTime TransferDate { get; set; }

        [Required]
        [StringLength(100)]
        public string PickupAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DropoffAddress { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int GuestId { get; set; }

        [Required]
        public int PersonnelId { get; set; }

        [Required]
        public int AirportId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        public string Note { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        public bool IsFromAirport { get; set; }

        [Required] // Yeni eklenen alan
        public int PickupCityId { get; set; }

        [Required] // Yeni eklenen alan
        public int DropoffCityId { get; set; }
    }
}
