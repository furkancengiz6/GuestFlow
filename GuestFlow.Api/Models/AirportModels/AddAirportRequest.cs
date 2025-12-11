using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.AirportModels
{
    public class AddAirportRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public int CityId { get; set; }
    }
}
