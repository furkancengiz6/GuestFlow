using GuestFlow.Application.Operations.DailyRevenue.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyRevenue
{
    public interface IDailyRevenueService
    {
        Task<ServiceMessage> AddDailyRevenue(AddDailyRevenueDto dailyRevenue);
        Task<ServiceMessage> UpdateDailyRevenue(UpdateDailyRevenueDto dailyRevenue);
        Task<ServiceMessage> DeleteDailyRevenue(int id);
        Task<GetDailyRevenueDto> GetDailyRevenueById(int id);
        Task<List<GetDailyRevenueDto>> GetDailyRevenues();
    }
}