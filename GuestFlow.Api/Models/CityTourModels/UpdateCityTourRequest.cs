using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.CityToursModels
{
    public class UpdateCityTourRequest
    {
        [Required]
        public DateTime TourDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Language { get; set; }

        [Required]
        [Range(1, 24)]
        public int DurationHours { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int OwnerGuestId { get; set; }

        [Required]
        public int PersonnelId { get; set; }

        [Required] // Yeni eklenen alan
        public int CityId { get; set; }
    }
}
