using GuestFlow.Api.Models.CityToursModels;
using GuestFlow.Application.Operations.CityTour;
using GuestFlow.Application.Operations.CityTour.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    public class CityToursController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _cityTourService: Şehir turlarıyla ilgili işlemleri yapmak için kullanıyorum.
        private readonly ICityTourService _cityTourService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public CityToursController(ICityTourService cityTourService)
        {
            _cityTourService = cityTourService;
        }

        // Bu metodumla yeni bir şehir turu ekliyorum.
        [HttpPost]
        public async Task<IActionResult> AddCityTour(AddCityTourRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var dto = new AddCityTourDto
            {
                TourDate = request.TourDate,
                Language = request.Language,
                DurationHours = request.DurationHours,
                Price = request.Price,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId,
                CityId = request.CityId,
                CreateInvoice = request.CreateInvoice,
                DiscountPercentage = request.DiscountPercentage,
                InvoiceDescription = request.InvoiceDescription
            };

            // Şehir turunu eklemek için servisi çağırıyorum.
            var result = await _cityTourService.AddCityTour(dto);
            // Eğer işlem başarılıysa, başarı mesajını JSON formatında döndürüyorum; değilse hata mesajı döndürüyorum.
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla belirli bir şehir turunu ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten şehir turunu ID'sine göre alıyorum.
            var result = await _cityTourService.GetCityTourById(id);
            // Eğer şehir turu bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound(new { Message = "Şehir turu bulunamadı." }) : Ok(result);
        }

        // Bu metodumla tüm şehir turlarını getiriyorum.
        [HttpGet]
        public async Task<IActionResult> GetCityTours()
        {
            // Servisten tüm şehir turlarını alıyorum ve JSON formatında döndürüyorum.
            var result = await _cityTourService.GetCityTours();
            return Ok(result);
        }

        // Bu metodumla bir şehir turunu güncelliyorum.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCityTourRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateCityTourDto = new UpdateCityTourDto
            {
                Id = id,
                TourDate = request.TourDate,
                Language = request.Language,
                DurationHours = request.DurationHours,
                Price = request.Price,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId,
                CityId = request.CityId
            };

            // Şehir turunu güncellemek için servisi çağırıyorum.
            var result = await _cityTourService.UpdateCityTour(updateCityTourDto);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir şehir turunu siliyorum.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Şehir turunu silmek için servisi çağırıyorum.
            var result = await _cityTourService.DeleteCityTour(id);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }
    }
}