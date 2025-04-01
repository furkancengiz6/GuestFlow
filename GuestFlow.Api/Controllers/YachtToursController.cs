using GuestFlow.Api.Models.YachtTourModels;
using GuestFlow.Application.Operations.YachtTour.Dtos;
using GuestFlow.Application.Operations.YachtTour;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class YachtToursController : ControllerBase
    {
        private readonly IYachtTourService _yachtTourService;

        public YachtToursController(IYachtTourService yachtTourService)
        {
            _yachtTourService = yachtTourService;
        }

        [HttpPost]
        public async Task<IActionResult> Add( AddYachtTourRequest request)
        {
            // Model doğrulama kontrolü
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Hatalı istek durumunda 400 döner
            }

            // Request modelini DTO'ya çevirme
            var dto = new AddYachtTourDto
            {
                TourDate = request.TourDate,
                NumberOfPeople = request.NumberOfPeople,
                Price = request.Price,
                SpecialRequest = request.SpecialRequest,
                YachtName = request.YachtName,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId,
                CityId = request.CityId,
                CreateInvoice = request.CreateInvoice, // Fatura oluşturma isteği
                DiscountPercentage = request.DiscountPercentage, // İndirim yüzdesi
                InvoiceDescription = request.InvoiceDescription // Fatura açıklaması
            };

            // Service katmanını çağırarak yat turu ekleme işlemini gerçekleştir
            var result = await _yachtTourService.AddYachtTour(dto);

            // Sonuca göre yanıt döndür
            if (result.IsSuccess)
            {
                return Ok(new { Message = result.Message }); // Başarılıysa 200 ve mesaj döner
            }
            else
            {
                return BadRequest(new { Message = result.Message }); // Başarısızsa 400 ve hata mesajı döner
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetYachtTours()
        {
            var result = await _yachtTourService.GetYachtTours();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _yachtTourService.GetYachtTourById(id);
            return result == null ? NotFound("Yacht turu bulunamadı.") : Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateYachtTourRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateYachtTourDto = new UpdateYachtTourDto
            {
                Id = id,
                TourDate = request.TourDate,
                NumberOfPeople = request.NumberOfPeople,
                Price = request.Price,
                SpecialRequest = request.SpecialRequest,
                YachtName = request.YachtName,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId,
                CityId = request.CityId // Yeni eklenen alan
            };

            var result = await _yachtTourService.UpdateYachtTour(updateYachtTourDto);
            return result.IsSuccess ? Ok() : BadRequest(result.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _yachtTourService.DeleteYachtTour(id);
            return result.IsSuccess ? Ok() : BadRequest(result.Message);
        }
    }
}
