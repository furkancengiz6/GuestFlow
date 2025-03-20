using GuestFlow.Domain.Entities.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
    public class CityEntity:BaseEntity,ICity
    {
        public string CityName { get; set; }  
        public string Country { get; set; }  
        
        public virtual ICollection<AirportEntity> Airports { get; set; } = new List<AirportEntity>();
        public CityEntity()
        {
        }//ilerde hata almamak için ekliyorum çakışma olmaması için eklemesekde otomotik ctor oluşturuyor.
    }
}
