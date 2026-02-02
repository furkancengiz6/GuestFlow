using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.Notification.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Misafir QR kod işlemleri için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("Misafir QR")]
    public class GuestQrController : BaseController
    {
        private readonly IGuestService _guestService;
        private readonly INotificationService _notificationService;

        public GuestQrController(IGuestService guestService, INotificationService notificationService)
        {
            _guestService = guestService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// QR kodu doğrular ve misafir bilgilerini döndürür
        /// </summary>
        /// <param name="code">QR kod içeriği (GuestCode)</param>
        [HttpGet("validate/{code}")]
        [ProducesResponseType(typeof(ApiResponse<GetGuestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ValidateGuestQr(string code)
        {
            var guest = await _guestService.GetGuestByCodeAsync(code);

            if (guest == null)
            {
                return NotFound("Geçersiz QR kod veya misafir bulunamadı.");
            }

            // VIP Misafir takibi için bildirim tetikle
            if (guest.IsSpecialGuest)
            {
                var personnelName = User.Identity?.Name ?? "Bir personel";
                
                await _notificationService.CreateAndSendNotificationAsync(new CreateNotificationDto
                {
                    Title = "⭐ VIP MİSAFİR TARANDI",
                    Content = $"VIP Misafir {guest.FullName}, şu an {personnelName} tarafından QR okutularak kontrol edildi. Lütfen öncelikli hizmet sağlayın.",
                    NotificationType = "Push",
                    RelatedEntityType = "Guest",
                    RelatedEntityId = guest.Id
                });
            }

            return Success(guest, "QR kod başarıyla doğrulandı.");
        }
    }
}
