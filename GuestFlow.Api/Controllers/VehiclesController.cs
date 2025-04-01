using GuestFlow.Api.Models.VehicleModels;
using GuestFlow.Application.Operations.Vehicle;
using GuestFlow.Application.Operations.Vehicle.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    public class VehiclesController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _vehicleService: Araçlarla ilgili işlemleri yapmak için kullanıyorum.
        private readonly IVehicleService _vehicleService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // Bu metodumla yeni bir araç ekliyorum.
        [HttpPost]
        public async Task<IActionResult> AddVehicle(AddVehicleRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var addVehicleDto = new AddVehicleDto
            {
                Type = request.Type,
                PlateNumber = request.PlateNumber,
                Capacity = request.Capacity,
                DailyPrice = request.DailyPrice
            };

            // Aracı eklemek için servisi çağırıyorum.
            var result = await _vehicleService.AddVehicle(addVehicleDto);
            // Eğer işlem başarılıysa, başarı mesajını JSON formatında döndürüyorum; değilse hata mesajı döndürüyorum.
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla tüm araçları getiriyorum.
        [HttpGet]
        public async Task<IActionResult> GetVehicles()
        {
            // Servisten tüm araçları alıyorum ve JSON formatında döndürüyorum.
            var result = await _vehicleService.GetVehicles();
            return Ok(result);
        }

        // Bu metodumla belirli bir aracı ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten aracı ID'sine göre alıyorum.
            var result = await _vehicleService.GetVehicleById(id);
            // Eğer araç bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound(new { Message = "Araç bulunamadı." }) : Ok(result);
        }

        // Bu metodumla bir aracı güncelliyorum.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateVehicleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateVehicleDto = new UpdateVehicleDto
            {
                Id = id,
                Type = request.Type,
                PlateNumber = request.PlateNumber,
                Capacity = request.Capacity,
                DailyPrice = request.DailyPrice
            };

            // Aracı güncellemek için servisi çağırıyorum.
            var result = await _vehicleService.UpdateVehicle(updateVehicleDto);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir aracı siliyorum.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Aracı silmek için servisi çağırıyorum.
            var result = await _vehicleService.DeleteVehicle(id);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }
    }
}