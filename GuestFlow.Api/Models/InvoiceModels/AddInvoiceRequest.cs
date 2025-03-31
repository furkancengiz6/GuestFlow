using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.InvoiceModels
{
    public class AddInvoiceRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        public int GuestId { get; set; }

        public int? TransferId { get; set; }
        public int? CityTourId { get; set; }
        public int? YachtTourId { get; set; }
    }
}
