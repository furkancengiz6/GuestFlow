using GuestFlow.Domain.Entities.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
    public class TransferEntity : BaseEntity, ITransfer
    {
        public string PickupAdress { get; set; }
        public string DropoffAdress { get; set; }
        public DateTime TransferDate { get; set; }
        public decimal Price { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public bool IsFromAirport { get; set; }
        //RoomNumber için ayrı bir tablo ve bağlantı gerekiyor.
        public int GuestId { get; set; }
        public int PersonnelId { get; set; }
        public int AirportId { get; set; }
        public int VehicleId { get; set; }




        //Relational Property
        public virtual GuestEntity Guest {  get; set; }
        public virtual PersonnelEntity Personnel { get; set; }
        public virtual AirportEntity Airport { get; set; }
        public virtual VehicleEntity Vehicle { get; set; }

        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
    }
}
