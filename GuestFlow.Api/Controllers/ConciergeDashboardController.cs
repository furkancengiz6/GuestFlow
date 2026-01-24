// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Dashboard;
using GuestFlow.Application.Operations.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using GuestFlow.Application.Models.Responses;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Concierge Dashboard API endpoint'leri - PMS entegrasyonlu concierge operasyonları için
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")] // Admin ve Staff erişebilir
    [Tags("Concierge Dashboard")]
    public class ConciergeDashboardController : BaseController
    {
        private readonly IConciergeDashboardService _conciergeDashboardService;
        private readonly ISmartNotificationService _smartNotificationService;
        private readonly IQuickActionService _quickActionService;

        public ConciergeDashboardController(
            IConciergeDashboardService conciergeDashboardService,
            ISmartNotificationService smartNotificationService,
            IQuickActionService quickActionService)
        {
            _conciergeDashboardService = conciergeDashboardService;
            _smartNotificationService = smartNotificationService;
            _quickActionService = quickActionService;
        }

        /// <summary>
        /// Bugünkü check-in'leri getirir (PMS + GuestFlow birleşik)
        /// </summary>
        /// <returns>Bugünkü check-in'ler listesi</returns>
        /// <response code="200">Check-in'ler başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("check-ins/today")]
        [ProducesResponseType(typeof(ApiResponse<ConciergeCheckInOutDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTodayCheckIns()
        {
            try
            {
                var result = await _conciergeDashboardService.GetTodayCheckInsAsync();
                return Success(result, "Bugünkü check-in'ler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Check-in'ler getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bugünkü check-out'ları getirir (PMS + GuestFlow birleşik)
        /// </summary>
        /// <returns>Bugünkü check-out'lar listesi</returns>
        /// <response code="200">Check-out'lar başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("check-outs/today")]
        [ProducesResponseType(typeof(ApiResponse<ConciergeCheckInOutDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTodayCheckOuts()
        {
            try
            {
                var result = await _conciergeDashboardService.GetTodayCheckOutsAsync();
                return Success(result, "Bugünkü check-out'lar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Check-out'lar getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Aktif misafirleri getirir (PMS + GuestFlow birleşik)
        /// </summary>
        /// <returns>Aktif misafirler listesi</returns>
        /// <response code="200">Aktif misafirler başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("active-guests")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<ActiveGuestDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetActiveGuests()
        {
            try
            {
                var result = await _conciergeDashboardService.GetActiveGuestsAsync();
                return Success(result, "Aktif misafirler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Aktif misafirler getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Unified guest profile getirir (PMS + GuestFlow verileri birleşik)
        /// </summary>
        /// <param name="guestId">Misafir ID</param>
        /// <returns>Unified guest profile</returns>
        /// <response code="200">Guest profile başarıyla getirildi</response>
        /// <response code="404">Guest bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("guests/{guestId}/unified-profile")]
        [ProducesResponseType(typeof(ApiResponse<UnifiedGuestProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUnifiedGuestProfile(int guestId)
        {
            try
            {
                var result = await _conciergeDashboardService.GetUnifiedGuestProfileAsync(guestId);
                return Success(result, "Unified guest profile başarıyla getirildi.");
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return Error("Guest profile getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Yaklaşan servisleri getirir (bugün ve yarın)
        /// </summary>
        /// <returns>Yaklaşan servisler listesi</returns>
        /// <response code="200">Yaklaşan servisler başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("upcoming-services")]
        [ProducesResponseType(typeof(ApiResponse<UpcomingServicesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUpcomingServices()
        {
            try
            {
                var result = await _conciergeDashboardService.GetUpcomingServicesForTodayAsync();
                return Success(result, "Yaklaşan servisler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Yaklaşan servisler getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Guest history dashboard getirir (önceki konaklamalar, hizmet geçmişi, harcama analizi)
        /// </summary>
        /// <param name="guestId">Misafir ID</param>
        /// <returns>Guest history dashboard</returns>
        /// <response code="200">Guest history dashboard başarıyla getirildi</response>
        /// <response code="404">Guest bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("guests/{guestId}/history")]
        [ProducesResponseType(typeof(ApiResponse<GuestHistoryDashboardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestHistoryDashboard(int guestId)
        {
            try
            {
                var result = await _conciergeDashboardService.GetGuestHistoryDashboardAsync(guestId);
                return Success(result, "Guest history dashboard başarıyla getirildi.");
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return Error("Guest history dashboard getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Concierge dashboard özet getirir (tüm bilgileri birleştirir)
        /// </summary>
        /// <returns>Concierge dashboard özet</returns>
        /// <response code="200">Dashboard özet başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ConciergeDashboardSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConciergeDashboardSummary()
        {
            try
            {
                var result = await _conciergeDashboardService.GetConciergeDashboardSummaryAsync();
                return Success(result, "Concierge dashboard özet başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Dashboard özet getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafir durumu göstergelerini getirir (VIP, özel istekler, doğum günü, vb.)
        /// </summary>
        /// <returns>Misafir durumu göstergeleri listesi</returns>
        /// <response code="200">Misafir durumu göstergeleri başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("guest-status-indicators")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<GuestStatusIndicatorDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestStatusIndicators()
        {
            try
            {
                var result = await _conciergeDashboardService.GetGuestStatusIndicatorsAsync();
                return Success(result, "Misafir durumu göstergeleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir durumu göstergeleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Pre-Arrival bildirimleri gönder (check-in öncesi hoş geldin mesajı)
        /// </summary>
        [HttpPost("notifications/pre-arrival")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendPreArrivalNotifications([FromQuery] DateTime? targetDate = null)
        {
            try
            {
                var result = await _smartNotificationService.SendPreArrivalNotificationsAsync(targetDate);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Pre-arrival bildirimleri gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Arrival bildirimleri gönder (check-in sonrası bilgilendirme)
        /// </summary>
        [HttpPost("notifications/arrival")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendArrivalNotifications([FromQuery] DateTime? targetDate = null)
        {
            try
            {
                var result = await _smartNotificationService.SendArrivalNotificationsAsync(targetDate);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Arrival bildirimleri gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// During Stay bildirimleri gönder (hizmet hatırlatmaları)
        /// </summary>
        [HttpPost("notifications/during-stay")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendDuringStayNotifications()
        {
            try
            {
                var result = await _smartNotificationService.SendDuringStayNotificationsAsync();
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("During-stay bildirimleri gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Pre-Departure bildirimleri gönder (check-out öncesi veda mesajı)
        /// </summary>
        [HttpPost("notifications/pre-departure")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendPreDepartureNotifications([FromQuery] DateTime? targetDate = null)
        {
            try
            {
                var result = await _smartNotificationService.SendPreDepartureNotificationsAsync(targetDate);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Pre-departure bildirimleri gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Special Occasions bildirimleri gönder (doğum günü, yıldönümü)
        /// </summary>
        [HttpPost("notifications/special-occasions")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendSpecialOccasionNotifications([FromQuery] DateTime? targetDate = null)
        {
            try
            {
                var result = await _smartNotificationService.SendSpecialOccasionNotificationsAsync(targetDate);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Special occasion bildirimleri gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir misafir için özel bildirim gönder
        /// </summary>
        [HttpPost("notifications/custom/{guestId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendCustomNotification(
            int guestId,
            [FromBody] SendCustomNotificationRequest request)
        {
            try
            {
                var result = await _smartNotificationService.SendCustomNotificationAsync(
                    guestId, 
                    request.NotificationType, 
                    request.Message, 
                    request.Channel);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Özel bildirim gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim şablonlarını getir
        /// </summary>
        [HttpGet("notifications/templates")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<NotificationTemplateDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotificationTemplates([FromQuery] string? notificationType = null)
        {
            try
            {
                var result = await _smartNotificationService.GetNotificationTemplatesAsync(notificationType);
                return result.Success ? Success(result.Data, "Bildirim şablonları başarıyla getirildi.") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Bildirim şablonları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Transfer rezervasyonu oluştur (hızlı aksiyon)
        /// </summary>
        [HttpPost("quick-actions/guests/{guestId}/transfer")]
        [ProducesResponseType(typeof(ApiResponse<QuickActionTransferResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateTransferReservation(int guestId, [FromBody] QuickActionTransferRequest request)
        {
            try
            {
                var result = await _quickActionService.CreateTransferReservationAsync(guestId, request);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Transfer rezervasyonu oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tur rezervasyonu oluştur (hızlı aksiyon)
        /// </summary>
        [HttpPost("quick-actions/guests/{guestId}/tour")]
        [ProducesResponseType(typeof(ApiResponse<QuickActionTourResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateTourReservation(int guestId, [FromBody] QuickActionTourRequest request)
        {
            try
            {
                var result = await _quickActionService.CreateTourReservationAsync(guestId, request);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Tur rezervasyonu oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Restoran rezervasyonu oluştur (hızlı aksiyon)
        /// </summary>
        [HttpPost("quick-actions/guests/{guestId}/restaurant")]
        [ProducesResponseType(typeof(ApiResponse<QuickActionRestaurantResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRestaurantReservation(int guestId, [FromBody] QuickActionRestaurantRequest request)
        {
            try
            {
                var result = await _quickActionService.CreateRestaurantReservationAsync(guestId, request);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Restoran rezervasyonu oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Oda servisi talebi oluştur (hızlı aksiyon)
        /// </summary>
        [HttpPost("quick-actions/guests/{guestId}/room-service")]
        [ProducesResponseType(typeof(ApiResponse<QuickActionRoomServiceResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRoomServiceRequest(int guestId, [FromBody] QuickActionRoomServiceRequest request)
        {
            try
            {
                var result = await _quickActionService.CreateRoomServiceRequestAsync(guestId, request);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Oda servisi talebi oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Mesaj gönder (hızlı aksiyon)
        /// </summary>
        [HttpPost("quick-actions/guests/{guestId}/message")]
        [ProducesResponseType(typeof(ApiResponse<QuickActionMessageResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendMessage(int guestId, [FromBody] QuickActionMessageRequest request)
        {
            try
            {
                var result = await _quickActionService.SendMessageAsync(guestId, request);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Mesaj gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// PMS folio (fatura) görüntüle (hızlı aksiyon)
        /// </summary>
        [HttpGet("quick-actions/guests/{guestId}/folio")]
        [ProducesResponseType(typeof(ApiResponse<QuickActionFolioResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFolio(int guestId)
        {
            try
            {
                var result = await _quickActionService.GetFolioAsync(guestId);
                return result.Success ? Success(result.Data, result.Message) : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Folio getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Özel bildirim gönderme request modeli
    /// </summary>
    public class SendCustomNotificationRequest
    {
        public string NotificationType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Channel { get; set; } // Email, SMS, WhatsApp
    }
}
