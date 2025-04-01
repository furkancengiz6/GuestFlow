using GuestFlow.Api.Models.DailyNoteModels;
using GuestFlow.Application.Operations.DailyNote;
using GuestFlow.Application.Operations.DailyNote.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    public class DailyNotesController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _dailyNoteService: Günlük notlarla ilgili işlemleri yapmak için kullanıyorum.
        private readonly IDailyNoteService _dailyNoteService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public DailyNotesController(IDailyNoteService dailyNoteService)
        {
            _dailyNoteService = dailyNoteService;
        }

        // Bu metodumla yeni bir günlük not ekliyorum.
        [HttpPost]
        public async Task<IActionResult> Add(AddDailyNoteRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var dto = new AddDailyNoteDto
            {
                NoteDate = request.NoteDate,
                RoomNumber = request.RoomNumber,
                NoteText = request.NoteText,
                PersonnelId = request.PersonnelId
            };

            // Günlük notu eklemek için servisi çağırıyorum.
            var result = await _dailyNoteService.AddDailyNote(dto);
            // Eğer işlem başarılıysa, başarı mesajını JSON formatında döndürüyorum; değilse hata mesajı döndürüyorum.
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir günlük notu güncelliyorum.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDailyNoteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var dto = new UpdateDailyNoteDto
            {
                Id = id,
                NoteDate = request.NoteDate,
                RoomNumber = request.RoomNumber,
                NoteText = request.NoteText,
                PersonnelId = request.PersonnelId
            };

            // Günlük notu güncellemek için servisi çağırıyorum.
            var result = await _dailyNoteService.UpdateDailyNote(dto);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir günlük notu siliyorum.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Günlük notu silmek için servisi çağırıyorum.
            var result = await _dailyNoteService.DeleteDailyNote(id);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla belirli bir günlük notu ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten günlük notu ID'sine göre alıyorum.
            var result = await _dailyNoteService.GetDailyNoteById(id);
            // Eğer günlük not bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound(new { Message = "Günlük not bulunamadı." }) : Ok(result);
        }

        // Bu metodumla tüm günlük notları getiriyorum.
        [HttpGet]
        public async Task<IActionResult> GetDailyNotes()
        {
            // Servisten tüm günlük notları alıyorum ve JSON formatında döndürüyorum.
            var result = await _dailyNoteService.GetDailyNotes();
            return Ok(result);
        }
    }
}