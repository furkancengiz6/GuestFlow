using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.CityModels
{
    public class AddCityRequest
    {
        [Required]
        [StringLength(100)]
        public string CityName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;
    }
}
