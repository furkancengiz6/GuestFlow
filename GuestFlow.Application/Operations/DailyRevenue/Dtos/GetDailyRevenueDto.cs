using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyRevenue.Dtos
{
    public class GetDailyRevenueDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}