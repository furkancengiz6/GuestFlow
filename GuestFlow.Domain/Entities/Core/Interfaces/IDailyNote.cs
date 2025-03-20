using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core.Interfaces
{
    public interface IDailyNote
    {
        DateTime NoteDate { get; set; }
        int RoomNumber { get; set; }
        string NoteText { get; set; }

    }
}
