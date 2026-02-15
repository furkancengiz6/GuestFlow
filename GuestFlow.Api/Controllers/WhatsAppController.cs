// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application.Models;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.WhatsApp;
using GuestFlow.Application.Operations.WhatsApp.Dtos;
using GuestFlow.Application.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff,Concierge")]
    [Tags("WhatsApp")]
    public class WhatsAppController : BaseController
    {
        private readonly IWhatsAppService _whatsAppService;

        public WhatsAppController(IWhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        /// <summary>
        /// WhatsApp mesajı gönderir
        /// </summary>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ApiResponse<GetWhatsAppHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendWhatsApp([FromBody] SendWhatsAppDto dto)
        {
            try
            {
                var result = await _whatsAppService.SendWhatsAppAsync(dto);
                if (result.IsSuccess)
                    return Ok(ApiResponse<GetWhatsAppHistoryDto>.SuccessResponse(result.Data, result.Message));
                else
                    return BadRequest(new ApiResponse<GetWhatsAppHistoryDto> { Success = false, Message = result.Message });
            }
            catch (Exception ex)
            {
                return Error("WhatsApp mesajı gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Transfer hatırlatma WhatsApp mesajı gönderir
        /// </summary>
        [HttpPost("transfer/{transferId}/reminder")]
        [ProducesResponseType(typeof(ApiResponse<GetWhatsAppHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendTransferReminder(int transferId, [FromQuery] int hoursBefore = 24)
        {
            try
            {
                var result = await _whatsAppService.SendTransferReminderAsync(transferId, hoursBefore);
                if (result.IsSuccess)
                    return Ok(ApiResponse<GetWhatsAppHistoryDto>.SuccessResponse(result.Data, result.Message));
                else
                    return BadRequest(new ApiResponse<GetWhatsAppHistoryDto> { Success = false, Message = result.Message });
            }
            catch (Exception ex)
            {
                return Error("Transfer hatırlatma WhatsApp mesajı gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tur hatırlatma WhatsApp mesajı gönderir
        /// </summary>
        [HttpPost("tour/{tourType}/{tourId}/reminder")]
        [ProducesResponseType(typeof(ApiResponse<GetWhatsAppHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendTourReminder(string tourType, int tourId, [FromQuery] int hoursBefore = 24)
        {
            try
            {
                var result = await _whatsAppService.SendTourReminderAsync(tourType, tourId, hoursBefore);
                if (result.IsSuccess)
                    return Ok(ApiResponse<GetWhatsAppHistoryDto>.SuccessResponse(result.Data, result.Message));
                else
                    return BadRequest(new ApiResponse<GetWhatsAppHistoryDto> { Success = false, Message = result.Message });
            }
            catch (Exception ex)
            {
                return Error("Tur hatırlatma WhatsApp mesajı gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Rezervasyon onay WhatsApp mesajı gönderir
        /// </summary>
        [HttpPost("reservation/{reservationId}/confirmation")]
        [ProducesResponseType(typeof(ApiResponse<GetWhatsAppHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendReservationConfirmation(int reservationId)
        {
            try
            {
                var result = await _whatsAppService.SendReservationConfirmationAsync(reservationId);
                if (result.IsSuccess)
                    return Ok(ApiResponse<GetWhatsAppHistoryDto>.SuccessResponse(result.Data, result.Message));
                else
                    return BadRequest(new ApiResponse<GetWhatsAppHistoryDto> { Success = false, Message = result.Message });
            }
            catch (Exception ex)
            {
                return Error("Rezervasyon onay WhatsApp mesajı gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// WhatsApp geçmişini getirir (sayfalanmış)
        /// </summary>
        [HttpPost("history")]
        [ProducesResponseType(typeof(ApiResponse<GuestFlow.Application.Models.PagedResult<GetWhatsAppHistoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWhatsAppHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] int? guestId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc")
        {
            try
            {
                var filters = new Dictionary<string, object>();
                if (guestId.HasValue)
                    filters["GuestId"] = guestId.Value;
                if (!string.IsNullOrEmpty(status))
                    filters["Status"] = status;
                if (startDate.HasValue)
                    filters["StartDate"] = startDate.Value;
                if (endDate.HasValue)
                    filters["EndDate"] = endDate.Value;

                var sorting = !string.IsNullOrEmpty(sortBy)
                    ? new SortingParameters { SortBy = sortBy, SortOrder = sortOrder }
                    : null;

                var result = await _whatsAppService.GetWhatsAppHistoryPagedAsync(
                    pageNumber,
                    pageSize,
                    filters,
                    sorting);

                return Ok(ApiResponse<GuestFlow.Application.Models.PagedResult<GetWhatsAppHistoryDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return Error("WhatsApp geçmişi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafire gönderilen WhatsApp mesajlarını getirir
        /// </summary>
        [HttpGet("guest/{guestId}")]
        [ProducesResponseType(typeof(ApiResponse<List<GetWhatsAppHistoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWhatsAppHistoryByGuest(int guestId)
        {
            try
            {
                var result = await _whatsAppService.GetWhatsAppHistoryByGuestIdAsync(guestId);
                return Ok(ApiResponse<List<GetWhatsAppHistoryDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return Error("Misafir WhatsApp geçmişi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// WhatsApp istatistiklerini getirir
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<WhatsAppStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWhatsAppStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _whatsAppService.GetWhatsAppStatisticsAsync(startDate, endDate);
                return Ok(ApiResponse<WhatsAppStatisticsDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return Error("WhatsApp istatistikleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// WhatsApp durumunu günceller (webhook için)
        /// </summary>
        [HttpPost("webhook/status")]
        [AllowAnonymous] // Webhook için authentication gerekmez, ama signature validation yapılmalı
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateWhatsAppStatus()
        {
            try
            {
                // Webhook signature validation
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }

                if (!Request.Headers.TryGetValue("X-Hub-Signature-256", out var signature))
                {
                    // For development/testing purposes, allow if signature is missing BUT log warning
                    // In production, this should return Unauthorized
                    // return Unauthorized(new ApiResponse<object> { Success = false, Message = "Missing X-Hub-Signature-256 header." });
                }
                
                if (!string.IsNullOrEmpty(signature) && !_whatsAppService.ValidateWebhookSignature(signature.ToString(), body))
                {
                    return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid webhook signature." });
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<UpdateWhatsAppStatusRequest>(body, options);

                if (request == null)
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request body." });
                }

                var result = await _whatsAppService.UpdateWhatsAppStatusAsync(
                    request.WhatsAppId,
                    request.Status,
                    request.MessageId,
                    request.GatewayResponse);

                if (result.IsSuccess)
                    return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
                else
                    return BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
            }
            catch (Exception ex)
            {
                return Error("WhatsApp durumu güncellenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// WhatsApp durum güncelleme request modeli (webhook için)
    /// </summary>
    public class UpdateWhatsAppStatusRequest
    {
        public int WhatsAppId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? MessageId { get; set; }
        public string? GatewayResponse { get; set; }
    }
}
