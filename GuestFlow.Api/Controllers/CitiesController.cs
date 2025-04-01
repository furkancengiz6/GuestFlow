using GuestFlow.Api.Models.CityModels;
using GuestFlow.Application.Operations.City;
using GuestFlow.Application.Operations.City.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    public class CitiesController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _cityService: Şehirlerle ilgili işlemleri yapmak için kullanıyorum.
        private readonly ICityService _cityService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public CitiesController(ICityService cityService)
        {
            _cityService = cityService;
        }

        // Bu metodumla yeni bir şehir ekliyorum.
        [HttpPost]
        public async Task<IActionResult> Add(AddCityRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var dto = new AddCityDto
            {
                CityName = request.CityName,
                Country = request.Country
            };

            // Şehri eklemek için servisi çağırıyorum.
            var result = await _cityService.AddCity(dto);
            // Eğer işlem başarılıysa, başarı mesajını JSON formatında döndürüyorum; değilse hata mesajı döndürüyorum.
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir şehri güncelliyorum.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var dto = new UpdateCityDto
            {
                Id = id,
                CityName = request.CityName,
                Country = request.Country
            };

            // Şehri güncellemek için servisi çağırıyorum.
            var result = await _cityService.UpdateCity(dto);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir şehri siliyorum.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Şehri silmek için servisi çağırıyorum.
            var result = await _cityService.DeleteCity(id);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla belirli bir şehri ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten şehri ID'sine göre alıyorum.
            var result = await _cityService.GetCityById(id);
            // Eğer şehir bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound(new { Message = "Şehir bulunamadı." }) : Ok(result);
        }

        // Bu metodumla tüm şehirleri getiriyorum.
        [HttpGet]
        public async Task<IActionResult> GetCities()
        {
            // Servisten tüm şehirleri alıyorum ve JSON formatında döndürüyorum.
            var result = await _cityService.GetCities();
            return Ok(result);
        }
    }
}