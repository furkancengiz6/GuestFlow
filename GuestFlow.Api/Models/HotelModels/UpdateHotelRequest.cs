using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.HotelModels
{
    public class UpdateHotelRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string HotelName { get; set; } = string.Empty;

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

        [Range(1, 5)]
        public int? StarRating { get; set; }

        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? RoomTypes { get; set; }
        public string? Amenities { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

