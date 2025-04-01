using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Interfaces
{
    public interface IDailyRevenue
    {
        DateTime Date { get; set; }
        decimal TotalRevenue { get; set; }
    }
}
