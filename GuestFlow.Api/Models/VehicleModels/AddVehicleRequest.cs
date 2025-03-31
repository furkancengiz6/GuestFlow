using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.VehicleModels
{
    public class AddVehicleRequest
    {
        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(20)]
        public string PlateNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal DailyPrice { get; set; }
    }
}
