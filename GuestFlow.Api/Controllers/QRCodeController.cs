using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.QRCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// QR kod oluşturma için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("QR Kod")]
    public class QRCodeController : BaseController
    {
        private readonly IQRCodeService _qrCodeService;

        public QRCodeController(IQRCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        /// <summary>
        /// Genel QR kod oluşturur
        /// </summary>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(ApiResponse<QRCodeResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateQRCode(
            [FromBody] GenerateQRCodeRequest request)
        {
            var result = await _qrCodeService.GenerateQRCodeAsync(request.Data, request.Size);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : BadRequest(result.Message);
        }

        /// <summary>
        /// Transfer için QR kod oluşturur
        /// </summary>
        [HttpGet("transfer/{transferId}")]
        [ProducesResponseType(typeof(ApiResponse<QRCodeResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateTransferQRCode(int transferId)
        {
            var result = await _qrCodeService.GenerateTransferQRCodeAsync(transferId);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : NotFound(result.Message);
        }

        /// <summary>
        /// İtinerary için QR kod oluşturur
        /// </summary>
        [HttpGet("itinerary/{itineraryId}")]
        [ProducesResponseType(typeof(ApiResponse<QRCodeResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateItineraryQRCode(int itineraryId)
        {
            var result = await _qrCodeService.GenerateItineraryQRCodeAsync(itineraryId);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : NotFound(result.Message);
        }

        /// <summary>
        /// Restoran rezervasyonu için QR kod oluşturur
        /// </summary>
        [HttpGet("restaurant-reservation/{reservationId}")]
        [ProducesResponseType(typeof(ApiResponse<QRCodeResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateRestaurantReservationQRCode(int reservationId)
        {
            var result = await _qrCodeService.GenerateRestaurantReservationQRCodeAsync(reservationId);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : NotFound(result.Message);
        }
    }

    public class GenerateQRCodeRequest
    {
        public string Data { get; set; } = string.Empty;
        public int Size { get; set; } = 300;
    }
}

