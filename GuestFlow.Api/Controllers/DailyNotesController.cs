using GuestFlow.Api.Models.DailyNoteModels;
using GuestFlow.Application.Operations.DailyNote;
using GuestFlow.Application.Operations.DailyNote.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class DailyNotesController : ControllerBase
    {
        private readonly IDailyNoteService _dailyNoteService;

        public DailyNotesController(IDailyNoteService dailyNoteService)
        {
            _dailyNoteService = dailyNoteService;
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddDailyNoteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new AddDailyNoteDto
            {
                NoteDate = request.NoteDate,
                RoomNumber = request.RoomNumber,
                NoteText = request.NoteText,
                PersonnelId = request.PersonnelId
            };

            var result = await _dailyNoteService.AddDailyNote(dto);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDailyNoteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new UpdateDailyNoteDto
            {
                Id = id,
                NoteDate = request.NoteDate,
                RoomNumber = request.RoomNumber,
                NoteText = request.NoteText,
                PersonnelId = request.PersonnelId
            };

            var result = await _dailyNoteService.UpdateDailyNote(dto);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _dailyNoteService.DeleteDailyNote(id);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _dailyNoteService.GetDailyNoteById(id);
            return result == null ? NotFound("Günlük not bulunamadı.") : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyNotes()
        {
            var result = await _dailyNoteService.GetDailyNotes();
            return Ok(result);
        }
    }
}