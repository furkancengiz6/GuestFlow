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
    [Authorize(Roles = "Staff,Admin")]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService _transferService;

        public TransfersController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpPost]
        public async Task<IActionResult> Add( AddTransferRequest request)
        {
            // Model doğrulama kontrolü
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Hatalı istek durumunda 400 döner
            }

            // Request modelini DTO'ya çevirme
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
                CreateInvoice = request.CreateInvoice, // Fatura oluşturma isteği
                DiscountPercentage = request.DiscountPercentage, // İndirim yüzdesi
                InvoiceDescription = request.InvoiceDescription // Fatura açıklaması
            };

            // Service katmanını çağırarak transfer ekleme işlemini gerçekleştir
            var result = await _transferService.AddTransfer(dto);

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
        public async Task<IActionResult> GetTransfers()
        {
            var result = await _transferService.GetTransfers();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _transferService.GetTransferById(id);
            if (result == null)
            {
                return NotFound("Transfer bulunamadı.");
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTransferRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
                PickupCityId = request.PickupCityId, // Yeni eklenen alan
                DropoffCityId = request.DropoffCityId // Yeni eklenen alan
            };

            var result = await _transferService.UpdateTransfer(updateTransferDto);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _transferService.DeleteTransfer(id);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
    }
}