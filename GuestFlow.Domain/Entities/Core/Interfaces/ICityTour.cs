using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core.Interfaces
{
    public interface ICityTour
    {
        DateTime TourDate { get; set; }
        string Language {  get; set; }
        int DurationHours { get; set; }
        decimal Price { get; set; }

    }
}
