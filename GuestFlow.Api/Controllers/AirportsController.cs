using GuestFlow.Api.Models.AirportModels;
using GuestFlow.Application.Operations.Airport.Dtos;
using GuestFlow.Application.Operations.Airport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirportsController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _airportService: Havalimanıyla ilgili işlemleri yapmak için kullanıyorum.
        private readonly IAirportService _airportService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public AirportsController(IAirportService airportService)
        {
            _airportService = airportService;
        }

        // Bu metodumla yeni bir havalimanı ekliyorum.
        [HttpPost]
        public async Task<IActionResult> AddAirport(AddAirportRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var addAirportDto = new AddAirportDto
            {
                Name = request.Name,
                Code = request.Code,
                CityId = request.CityId
            };

            // Havalimanını eklemek için servisi çağırıyorum.
            var result = await _airportService.AddAirport(addAirportDto);
            if (result.IsSuccess)
            {
                // Eğer işlem başarılıysa, başarı mesajını JSON formatında döndürüyorum.
                return Ok(new { Message = result.Message });
            }
            else
            {
                // Eğer işlem başarısızsa, hata mesajını JSON formatında döndürüyorum.
                return BadRequest(new { Message = result.Message });
            }
        }

        // Bu metodumla tüm havalimanlarını getiriyorum.
        [HttpGet]
        [Authorize(Roles = "Staff,Admin")] // Bu endpoint'e sadece Staff ve Admin rolleri erişebilir.
        public async Task<IActionResult> GetAirports()
        {
            // Servisten tüm havalimanlarını alıyorum.
            var result = await _airportService.GetAirports();
            // Sonucu JSON formatında döndürüyorum.
            return Ok(result);
        }

        // Bu metodumla belirli bir havalimanını ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten havalimanını ID'sine göre alıyorum.
            var result = await _airportService.GetAirportById(id);
            if (result == null)
            {
                // Eğer havalimanı bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum.
                return NotFound(new { Message = "Havalimanı bulunamadı." });
            }
            // Havalimanı bulunursa, sonucu JSON formatında döndürüyorum.
            return Ok(result);
        }

        // Bu metodumla bir havalimanını güncelliyorum.
        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Admin")] 
        public async Task<IActionResult> Update(int id, UpdateAirportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateAirportDto = new UpdateAirportDto
            {
                Id = id,
                Name = request.Name,
                Code = request.Code,
                CityId = request.CityId
            };

            // Havalimanını güncellemek için servisi çağırıyorum.
            var result = await _airportService.UpdateAirport(updateAirportDto);
            if (result.IsSuccess)
            {
                return Ok(new { Message = result.Message });
            }
            else
            {
                return BadRequest(new { Message = result.Message });
            }
        }

        // Bu metodumla bir havalimanını siliyorum.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Admin")] 
        public async Task<IActionResult> Delete(int id)
        {
            // Havalimanını silmek için servisi çağırıyorum.
            var result = await _airportService.DeleteAirport(id);
            if (result.IsSuccess)
            {
                return Ok(new { Message = result.Message });
            }
            else
            {
                return BadRequest(new { Message = result.Message });
            }
        }
    }
}