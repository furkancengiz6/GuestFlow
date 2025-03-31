using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.CityToursModels
{
    public class AddCityTourRequest
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

        [Required] 
        public int CityId { get; set; }
        public bool CreateInvoice { get; set; } // Fatura oluşturulacak mı?
        public decimal? DiscountPercentage { get; set; } //  İndirim yüzdesi
        public string? InvoiceDescription { get; set; }
    }
}
