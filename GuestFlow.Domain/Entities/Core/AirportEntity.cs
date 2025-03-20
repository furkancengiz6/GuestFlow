using GuestFlow.Domain.Entities.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
    public class AirportEntity:BaseEntity,IAirport
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int CityId { get; set; }


        //Relational Property
        public virtual ICollection<TransferEntity> Transfers { get; set; }
        public virtual CityEntity City { get; set; }

        public AirportEntity() { }
    }
}
