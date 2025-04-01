using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.DailyRevenueModels
{
    public class AddDailyRevenueRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalRevenue { get; set; }
    }
}