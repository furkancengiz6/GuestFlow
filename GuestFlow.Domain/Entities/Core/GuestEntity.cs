using GuestFlow.Domain.Entities.Core.Interfaces;
using GuestFlow.Domain.Entities.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
    public class GuestEntity : BaseEntity, IGuest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Nationality { get; set; }


        //Relational Property
        public virtual ICollection<TransferEntity> Transfers { get; set; } = new List<TransferEntity>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
        public virtual ICollection<GuestYachtTour> GuestYachtTours { get; set; } = new List<GuestYachtTour>();
        public virtual ICollection<GuestCityTour> GuestCityTours { get; set; } = new List<GuestCityTour>();
        public virtual ICollection<YachtTourEntity> YachtTours { get; set; } = new List<YachtTourEntity>();
    }
}


