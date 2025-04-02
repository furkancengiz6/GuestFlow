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
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    public class YachtToursController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _yachtTourService: Yat turlarıyla ilgili işlemleri yapmak için kullanıyorum.
        private readonly IYachtTourService _yachtTourService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public YachtToursController(IYachtTourService yachtTourService)
        {
            _yachtTourService = yachtTourService;
        }

        // Bu metodumla yeni bir yat turu ekliyorum.
        [HttpPost]
        public async Task<IActionResult> Add(AddYachtTourRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
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
                CreateInvoice = request.CreateInvoice,
                DiscountPercentage = request.DiscountPercentage,
                InvoiceDescription = request.InvoiceDescription
            };

            // Yat turunu eklemek için servisi çağırıyorum.
            var result = await _yachtTourService.AddYachtTour(dto);
            // Eğer işlem başarılıysa, başarı mesajını JSON formatında döndürüyorum; değilse hata mesajı döndürüyorum.
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla tüm yat turlarını getiriyorum.
        [HttpGet]
        public async Task<IActionResult> GetYachtTours()
        {
            // Servisten tüm yat turlarını alıyorum ve JSON formatında döndürüyorum.
            var result = await _yachtTourService.GetYachtTours();
            return Ok(result);
        }

        // Bu metodumla belirli bir yat turunu ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Test için bir hata fırlatıyorum.
            if (id == 999)
                throw new ArgumentException("Geçersiz bir ID değeri: 999");
            // Servisten yat turunu ID'sine göre alıyorum.
            var result = await _yachtTourService.GetYachtTourById(id);
            // Eğer yat turu bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound(new { Message = "Yat turu bulunamadı." }) : Ok(result);
        }

        // Bu metodumla bir yat turunu güncelliyorum.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateYachtTourRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
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
                CityId = request.CityId
            };

            // Yat turunu güncellemek için servisi çağırıyorum.
            var result = await _yachtTourService.UpdateYachtTour(updateYachtTourDto);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir yat turunu siliyorum.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Yat turunu silmek için servisi çağırıyorum.
            var result = await _yachtTourService.DeleteYachtTour(id);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }
    }
}