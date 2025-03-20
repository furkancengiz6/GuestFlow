using GuestFlow.Domain.Entities.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
  public class DailyNote:BaseEntity,IDailyNote
    {
       public DateTime NoteDate { get; set; }
       public int RoomNumber { get; set; }
      public  string NoteText { get; set; }
        public int PersonnelId { get; set; }
        //Relational Property
        public virtual PersonnelEntity Personnel { get; set; }
    }
}
