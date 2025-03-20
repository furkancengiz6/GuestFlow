using GuestFlow.Domain.Entities.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
   public class PersonnelEntity:BaseEntity,IPersonnel
    {

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }


        //Relational Property
        public virtual ICollection<TransferEntity> Transfers { get; set; } = new List<TransferEntity>();
        public virtual ICollection<YachtTourEntity> YachtTours { get; set; } = new List<YachtTourEntity>(); 
        public virtual ICollection<CityTourEntity> CityTours { get; set; } = new List<CityTourEntity>();
        public virtual ICollection<DailyNote> DailyNotes { get; set; } = new List<DailyNote>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();

    }
}
