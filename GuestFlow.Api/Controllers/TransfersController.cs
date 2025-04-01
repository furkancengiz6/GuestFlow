using GuestFlow.Api.Models.TransferModel;
using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Operations.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    public class TransfersController : ControllerBase
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _transferService: Transfer işlemleriyle ilgili işlemleri yapmak için kullanıyorum.
        private readonly ITransferService _transferService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public TransfersController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        // Bu metodumla yeni bir transfer kaydı ekliyorum.
        [HttpPost]
        public async Task<IActionResult> Add(AddTransferRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var dto = new AddTransferDto
            {
                TransferDate = request.TransferDate,
                PickupAddress = request.PickupAddress,
                DropoffAddress = request.DropoffAddress,
                Price = request.Price,
                GuestId = request.GuestId,
                PersonnelId = request.PersonnelId,
                AirportId = request.AirportId,
                VehicleId = request.VehicleId,
                Note = request.Note,
                Status = request.Status,
                IsFromAirport = request.IsFromAirport,
                PickupCityId = request.PickupCityId,
                DropoffCityId = request.DropoffCityId,
                CreateInvoice = request.CreateInvoice,
                DiscountPercentage = request.DiscountPercentage,
                InvoiceDescription = request.InvoiceDescription
            };

            // Transferi eklemek için servisi çağırıyorum.
            var result = await _transferService.AddTransfer(dto);
            // Eğer işlem başarılıysa, başarı mesajını JSON formatında döndürüyorum; değilse hata mesajı döndürüyorum.
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla tüm transferleri getiriyorum.
        [HttpGet]
        public async Task<IActionResult> GetTransfers()
        {
            // Servisten tüm transferleri alıyorum ve JSON formatında döndürüyorum.
            var result = await _transferService.GetTransfers();
            return Ok(result);
        }

        // Bu metodumla belirli bir transferi ID'sine göre getiriyorum.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten transferi ID'sine göre alıyorum.
            var result = await _transferService.GetTransferById(id);
            // Eğer transfer bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound(new { Message = "Transfer bulunamadı." }) : Ok(result);
        }

        // Bu metodumla bir transferi güncelliyorum.
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTransferRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateTransferDto = new UpdateTransferDto
            {
                Id = id,
                TransferDate = request.TransferDate,
                PickupAddress = request.PickupAddress,
                DropoffAddress = request.DropoffAddress,
                Price = request.Price,
                GuestId = request.GuestId,
                PersonnelId = request.PersonnelId,
                AirportId = request.AirportId,
                VehicleId = request.VehicleId,
                Note = request.Note,
                Status = request.Status,
                IsFromAirport = request.IsFromAirport,
                PickupCityId = request.PickupCityId,
                DropoffCityId = request.DropoffCityId
            };

            // Transferi güncellemek için servisi çağırıyorum.
            var result = await _transferService.UpdateTransfer(updateTransferDto);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        // Bu metodumla bir transferi siliyorum.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Transferi silmek için servisi çağırıyorum.
            var result = await _transferService.DeleteTransfer(id);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }
    }
}