using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.RestaurantModels
{
    public class AddRestaurantRequest
    {
        [Required]
        [StringLength(200)]
        public string RestaurantName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [Required]
        public int CityId { get; set; }

        [StringLength(100)]
        public string? CuisineType { get; set; }

        [Range(1, int.MaxValue)]
        public int? Capacity { get; set; }

        [StringLength(500)]
        public string? OperatingHours { get; set; }

        public bool ReservationRequired { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

