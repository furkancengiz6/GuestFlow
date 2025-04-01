using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyNote.Dtos
{
    public class GetDailyNoteDto
    {
        public int Id { get; set; }
        public DateTime NoteDate { get; set; }
        public int RoomNumber { get; set; }
        public string NoteText { get; set; }
        public int? PersonnelId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}