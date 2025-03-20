using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core.Interfaces
{
    public interface IYachtTour
    {
        DateTime TourDate { get; set; }
        int NumberOfPeople { get; set; }
        decimal Price { get; set; }
        string SpecialRequest { get; set; }

    }
}
