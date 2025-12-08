using GuestFlow.Api.Models.DailyRevenueModels;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.DailyRevenue.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    public class DailyRevenuesController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _dailyRevenueService: Günlük gelirlerle ilgili işlemleri yapmak için kullanıyorum.
        private readonly IDailyRevenueService _dailyRevenueService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public DailyRevenuesController(IDailyRevenueService dailyRevenueService)
        {
            _dailyRevenueService = dailyRevenueService;
        }

        // Bu metodumla yeni bir günlük gelir kaydı ekliyorum.
        [HttpPost]
        public async Task<IActionResult> Add(AddDailyRevenueRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var dto = new AddDailyRevenueDto
            {
                Date = request.Date,
                TotalRevenue = request.TotalRevenue
            };

            // Günlük geliri eklemek için servisi çağırıyorum.
            var result = await _dailyRevenueService.AddDailyRevenue(dto);
            // Eğer işlem başarılıysa, başarı mesajını JSON formatında döndürüyorum; değilse hata mesajı döndürüyorum.
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir günlük geliri güncelliyorum.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDailyRevenueRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var dto = new UpdateDailyRevenueDto
            {
                Id = id,
                Date = request.Date,
                TotalRevenue = request.TotalRevenue
            };

            // Günlük geliri güncellemek için servisi çağırıyorum.
            var result = await _dailyRevenueService.UpdateDailyRevenue(dto);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir günlük geliri siliyorum.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Günlük geliri silmek için servisi çağırıyorum.
            var result = await _dailyRevenueService.DeleteDailyRevenue(id);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla belirli bir günlük geliri ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten günlük geliri ID'sine göre alıyorum.
            var result = await _dailyRevenueService.GetDailyRevenueById(id);
            // Eğer günlük gelir bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound(new { Message = "Günlük gelir bulunamadı." }) : Ok(result);
        }

        // Bu metodumla tüm günlük gelirleri getiriyorum.
        [HttpGet]
        public async Task<IActionResult> GetDailyRevenues()
        {
            // Servisten tüm günlük gelirleri alıyorum ve JSON formatında döndürüyorum.
            var result = await _dailyRevenueService.GetDailyRevenues();
            return Ok(result);
        }
    }
}