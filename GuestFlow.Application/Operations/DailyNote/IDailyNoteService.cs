using GuestFlow.Application.Operations.DailyNote.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyNote
{
    public interface IDailyNoteService
    {
        Task<ServiceMessage> AddDailyNote(AddDailyNoteDto dailyNote);
        Task<ServiceMessage> UpdateDailyNote(UpdateDailyNoteDto dailyNote);
        Task<ServiceMessage> DeleteDailyNote(int id);
        Task<GetDailyNoteDto> GetDailyNoteById(int id);
        Task<List<GetDailyNoteDto>> GetDailyNotes();
    }
}