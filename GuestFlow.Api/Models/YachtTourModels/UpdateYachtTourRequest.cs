using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.YachtTourModels
{
    public class UpdateYachtTourRequest
    {
        [Required]
        public DateTime TourDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int NumberOfPeople { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string SpecialRequest { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string YachtName { get; set; } = string.Empty;

        [Required]
        public int OwnerGuestId { get; set; }

        [Required]
        public int PersonnelId { get; set; }

        [Required] 
        public int CityId { get; set; }
    }
}
