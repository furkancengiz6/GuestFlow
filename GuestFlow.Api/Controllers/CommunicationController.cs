// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Communication;
using GuestFlow.Application.Operations.Communication.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    [Tags("Misafir İletişim Merkezi")]
    public class CommunicationController : BaseController
    {
        private readonly IUnifiedCommunicationService _communicationService;

        public CommunicationController(IUnifiedCommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        /// <summary>
        /// Misafir için tüm iletişim geçmişini getirir (e-posta, SMS, WhatsApp, in-app)
        /// </summary>
        /// <param name="guestId">Misafir ID</param>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Unified Communication History</returns>
        /// <response code="200">İletişim geçmişi başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("guests/{guestId}/history")]
        [ProducesResponseType(typeof(ApiResponse<UnifiedCommunicationHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestCommunicationHistory(
            int guestId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _communicationService.GetGuestCommunicationHistoryAsync(guestId, startDate, endDate);
                if (result.Success && result.Data != null)
                {
                    return Success(result.Data, "İletişim geçmişi başarıyla getirildi.");
                }
                return Error(result.Message ?? "İletişim geçmişi getirilemedi.", 404);
            }
            catch (Exception ex)
            {
                return Error("İletişim geçmişi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafire mesaj gönderir (e-posta, SMS, WhatsApp)
        /// </summary>
        /// <param name="guestId">Misafir ID</param>
        /// <param name="dto">Mesaj bilgileri</param>
        /// <returns>Gönderim sonucu</returns>
        /// <response code="200">Mesaj başarıyla gönderildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("guests/{guestId}/send")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendMessage(int guestId, [FromBody] SendMessageDto dto)
        {
            try
            {
                var result = await _communicationService.SendMessageAsync(guestId, dto);
                if (result.Success)
                {
                    return Success(true, "Mesaj başarıyla gönderildi.");
                }
                return Error(result.Message ?? "Mesaj gönderilemedi.", 400);
            }
            catch (Exception ex)
            {
                return Error("Mesaj gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Smart notification gönderir (Pre-Arrival, Arrival, During Stay, Pre-Departure, Special Occasions)
        /// </summary>
        /// <param name="guestId">Misafir ID</param>
        /// <param name="notificationType">Notification tipi</param>
        /// <returns>Gönderim sonucu</returns>
        /// <response code="200">Smart notification başarıyla gönderildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("guests/{guestId}/smart-notification")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendSmartNotification(
            int guestId,
            [FromQuery] SmartNotificationType notificationType)
        {
            try
            {
                var result = await _communicationService.SendSmartNotificationAsync(guestId, notificationType);
                if (result.Success)
                {
                    return Success(true, "Smart notification başarıyla gönderildi.");
                }
                return Error(result.Message ?? "Smart notification gönderilemedi.", 400);
            }
            catch (Exception ex)
            {
                return Error("Smart notification gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}
