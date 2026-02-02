using System;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Finance.Revenue
{
    public interface IRevenueService
    {
        Task<RevenueDashboardDto> GetRevenueDashboardAsync(DateTime startDate, DateTime endDate);
    }

    public class RevenueDashboardDto
    {
         public decimal ADR { get; set; } // Average Daily Rate
         public decimal RevPAR { get; set; } // Revenue Per Available Room
         public decimal OccupancyRate { get; set; }
         public decimal TotalRevenue { get; set; }
         public int TotalRoomsSold { get; set; }
    }
}
