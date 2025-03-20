using GuestFlow.Domain.Entities.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
    public class DailyRevenueEntity:BaseEntity,IDailyRevenue
    {
      public  DateTime Date { get; set; }
       public decimal TotalRevenue { get; set; }
    }
}
