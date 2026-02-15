using GuestFlow.Application.Models.AI;
using GuestFlow.Application.Operations.AI;
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
        private readonly IAIAssistantService _aiAssistantService;

        public GuestQrController(IGuestService guestService, INotificationService notificationService, IAIAssistantService aiAssistantService)
        {
            _guestService = guestService;
            _notificationService = notificationService;
            _aiAssistantService = aiAssistantService;
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

            // AI Insight Oluştur (Personel için)
            string aiStaffTip = "Normal VIP protokolünü uygulayın.";
            try
            {
                var aiRequest = new AIChatRequest
                {
                    Message = $"Provide a 1-sentence personalized 'Staff Tip' for this guest: {guest.FullName}. They are VIP: {guest.IsSpecialGuest}. Consider nationality: {guest.Nationality}. Return in Turkish.",
                    GuestId = guest.Id
                };
                var aiResponse = await _aiAssistantService.ProcessMessageAsync(aiRequest);
                aiStaffTip = aiResponse.Response;
            }
            catch { /* Fallback to default */ }

            // VIP Misafir takibi için bildirim tetikle
            if (guest.IsSpecialGuest)
            {
                var personnelName = User.Identity?.Name ?? "Bir personel";
                
                await _notificationService.CreateAndSendNotificationAsync(new CreateNotificationDto
                {
                    Title = "⭐ VIP MİSAFİR TARANDI",
                    Content = $"VIP Misafir {guest.FullName}, {personnelName} tarafından kontrol edildi. AI Önerisi: {aiStaffTip}",
                    NotificationType = "Push",
                    RelatedEntityType = "Guest",
                    RelatedEntityId = guest.Id
                });
            }

            return Success(new { Guest = guest, AISuggestion = aiStaffTip }, "QR kod başarıyla doğrulandı.");
        }
    }
}
