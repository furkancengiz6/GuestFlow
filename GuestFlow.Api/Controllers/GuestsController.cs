using GuestFlow.Api.Models.GuestModels;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    public class GuestsController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _guestService: Misafirlerle ilgili işlemleri yapmak için kullanıyorum.
        private readonly IGuestService _guestService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public GuestsController(IGuestService guestService)
        {
            _guestService = guestService;
        }

        // Bu metodumla yeni bir misafir ekliyorum.
        [HttpPost]
        public async Task<IActionResult> AddGuest(AddGuestRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var addGuestDto = new AddGuestDto
            {
                FullName = request.FullName,
                Email = request.Email,
                Nationality = request.Nationality,
                PhoneNumber = request.PhoneNumber,
                IsSpecialGuest = request.IsSpecialGuest
            };

            // Misafiri eklemek için servisi çağırıyorum.
            var result = await _guestService.AddGuest(addGuestDto);
            // Eğer işlem başarılıysa, başarı mesajını JSON formatında döndürüyorum; değilse hata mesajı döndürüyorum.
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla tüm misafirleri getiriyorum.
        [HttpGet]
        public async Task<IActionResult> GetGuests()
        {
            // Servisten tüm misafirleri alıyorum ve JSON formatında döndürüyorum.
            var result = await _guestService.GetGuests();
            return Ok(result);
        }

        // Bu metodumla belirli bir misafiri ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuestById(int id)
        {
            // Servisten misafiri ID'sine göre alıyorum.
            var result = await _guestService.GetGuestById(id);
            // Eğer misafir bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound(new { Message = "Misafir bulunamadı." }) : Ok(result);
        }

        // Bu metodumla bir misafiri güncelliyorum.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateGuestRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateGuestDto = new UpdateGuestDto
            {
                Id = id,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Nationality = request.Nationality,
                IsSpecialGuest = request.IsSpecialGuest
            };

            // Misafiri güncellemek için servisi çağırıyorum.
            var result = await _guestService.UpdateGuest(updateGuestDto);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir misafiri siliyorum.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Misafiri silmek için servisi çağırıyorum.
            var result = await _guestService.DeleteGuest(id);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }
    }
}