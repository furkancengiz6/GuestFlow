using GuestFlow.Domain.Entities.Core.Interfaces;
using GuestFlow.Domain.Entities.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
    public class YachtTourEntity:BaseEntity,IYachtTour
    {
        public DateTime TourDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal Price { get; set; }
        public string SpecialRequest { get; set; }
        public int GuestId { get; set; }
        public int PersonnelId { get; set; }

        //Relational Property
        public virtual GuestEntity Guest {  get; set; }
        public virtual PersonnelEntity Personnel { get; set; }
        public virtual ICollection<GuestYachtTour> GuestYachtTours { get; set; } = new List<GuestYachtTour>(); //çoka çok
    }
}
