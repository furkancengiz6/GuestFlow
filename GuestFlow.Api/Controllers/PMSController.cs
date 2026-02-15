// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Requests.PMS;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Application.Models;
using GuestFlow.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v1.0/[controller]")]
    [Authorize]
    public class PMSController : BaseController
    {
        private readonly IPMSIntegrationService _pmsService;
        private readonly IPMSWebhookProcessor _webhookProcessor;
        private readonly IPMSSyncService _syncService;

        public PMSController(
            IPMSIntegrationService pmsService, 
            IPMSWebhookProcessor webhookProcessor,
            IPMSSyncService syncService)
        {
            _pmsService = pmsService;
            _webhookProcessor = webhookProcessor;
            _syncService = syncService;
        }

        /// <summary>
        /// Belirli bir misafiri PMS'den senkronize et (Manual Trigger)
        /// </summary>
        [HttpPost("integrations/{integrationId}/sync/guests/{pmsGuestId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> SyncGuest(int integrationId, string pmsGuestId)
        {
            var result = await _syncService.SyncGuestByIdAsync(integrationId, pmsGuestId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Belirli bir rezervasyonu PMS'den senkronize et (Manual Trigger)
        /// </summary>
        [HttpPost("integrations/{integrationId}/sync/reservations/{pmsReservationId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> SyncReservation(int integrationId, string pmsReservationId)
        {
            var result = await _syncService.SyncReservationByIdAsync(integrationId, pmsReservationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Belirli bir odanın durumunu PMS'den senkronize et (Manual Trigger)
        /// </summary>
        [HttpPost("integrations/{integrationId}/sync/rooms/{roomNumber}/status")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> SyncRoomStatus(int integrationId, string roomNumber)
        {
            var result = await _syncService.SyncRoomStatusByRoomNumberAsync(integrationId, roomNumber);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Belirli bir folio'yu (faturayı) PMS'den senkronize et (Manual Trigger)
        /// </summary>
        [HttpPost("integrations/{integrationId}/sync/folios/{pmsFolioId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> SyncFolio(int integrationId, string pmsFolioId)
        {
            var result = await _syncService.SyncFolioByIdAsync(integrationId, pmsFolioId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Tüm PMS entegrasyonlarını listele
        /// </summary>
        [HttpGet("integrations")]
        [ProducesResponseType(typeof(ApiResponse<List<PMSIntegration>>), 200)]
        public async Task<IActionResult> GetAllIntegrations()
        {
            var result = await _pmsService.GetAllPMSIntegrationsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PMS entegrasyonu oluştur
        /// </summary>
        [HttpPost("integrations")]
        [ProducesResponseType(typeof(ApiResponse<PMSIntegration>), 201)]
        public async Task<IActionResult> CreateIntegration([FromBody] CreatePMSIntegrationRequest request)
        {
            var result = await _pmsService.CreatePMSIntegrationAsync(request);
            return result.Success ? CreatedAtAction(nameof(GetIntegrationById), new { integrationId = result.Data?.Id }, result) : BadRequest(result);
        }

        /// <summary>
        /// PMS entegrasyonu detayını getir
        /// </summary>
        [HttpGet("integrations/{integrationId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSIntegration>), 200)]
        public async Task<IActionResult> GetIntegrationById(int integrationId)
        {
            var result = await _pmsService.GetPMSIntegrationByIdAsync(integrationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// PMS entegrasyonu güncelle
        /// </summary>
        [HttpPut("integrations/{integrationId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSIntegration>), 200)]
        public async Task<IActionResult> UpdateIntegration(int integrationId, [FromBody] UpdatePMSIntegrationRequest request)
        {
            var result = await _pmsService.UpdatePMSIntegrationAsync(integrationId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PMS entegrasyonu sil
        /// </summary>
        [HttpDelete("integrations/{integrationId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> DeleteIntegration(int integrationId)
        {
            var result = await _pmsService.DeletePMSIntegrationAsync(integrationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PMS bağlantısını test et
        /// </summary>
        [HttpPost("integrations/{integrationId}/test-connection")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> TestConnection(int integrationId)
        {
            var result = await _pmsService.TestPMSConnectionAsync(integrationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PMS access token'ı yenile
        /// </summary>
        [HttpPost("integrations/{integrationId}/refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> RefreshToken(int integrationId)
        {
            var result = await _pmsService.RefreshPMSAccessTokenAsync(integrationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PMS'den misafir profili getir
        /// </summary>
        [HttpGet("integrations/{integrationId}/guests/{pmsGuestId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSGuestProfile>), 200)]
        public async Task<IActionResult> GetGuestProfile(int integrationId, string pmsGuestId)
        {
            var result = await _pmsService.GetGuestProfileAsync(integrationId, pmsGuestId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// PMS'den misafirleri listele
        /// </summary>
        [HttpGet("integrations/{integrationId}/guests")]
        [ProducesResponseType(typeof(ApiResponse<List<PMSGuestProfile>>), 200)]
        public async Task<IActionResult> GetGuests(int integrationId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _pmsService.GetGuestsAsync(integrationId, startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PMS'den rezervasyon getir
        /// </summary>
        [HttpGet("integrations/{integrationId}/reservations/{pmsReservationId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSReservation>), 200)]
        public async Task<IActionResult> GetReservation(int integrationId, string pmsReservationId)
        {
            var result = await _pmsService.GetReservationAsync(integrationId, pmsReservationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// PMS'den rezervasyonları listele
        /// </summary>
        [HttpGet("integrations/{integrationId}/reservations")]
        [ProducesResponseType(typeof(ApiResponse<List<PMSReservation>>), 200)]
        public async Task<IActionResult> GetReservations(int integrationId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _pmsService.GetReservationsAsync(integrationId, startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PMS'den oda durumu getir
        /// </summary>
        [HttpGet("integrations/{integrationId}/rooms/{roomNumber}/status")]
        [ProducesResponseType(typeof(ApiResponse<PMSRoomStatus>), 200)]
        public async Task<IActionResult> GetRoomStatus(int integrationId, string roomNumber)
        {
            var result = await _pmsService.GetRoomStatusAsync(integrationId, roomNumber);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// PMS'den oda durumlarını listele
        /// </summary>
        [HttpGet("integrations/{integrationId}/rooms/status")]
        [ProducesResponseType(typeof(ApiResponse<List<PMSRoomStatus>>), 200)]
        public async Task<IActionResult> GetRoomsStatus(int integrationId, [FromQuery] DateTime? date = null)
        {
            var result = await _pmsService.GetRoomsStatusAsync(integrationId, date);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// PMS'den folio (fatura) getir
        /// </summary>
        [HttpGet("integrations/{integrationId}/folios/{reservationId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSFolio>), 200)]
        public async Task<IActionResult> GetFolio(int integrationId, string reservationId)
        {
            var result = await _pmsService.GetFolioAsync(integrationId, reservationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// PMS'den folio'ları listele
        /// </summary>
        [HttpGet("integrations/{integrationId}/folios")]
        [ProducesResponseType(typeof(ApiResponse<List<PMSFolio>>), 200)]
        public async Task<IActionResult> GetFolios(int integrationId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _pmsService.GetFoliosAsync(integrationId, startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Senkronizasyon geçmişini getir
        /// </summary>
        [HttpGet("integrations/{integrationId}/sync-history")]
        [ProducesResponseType(typeof(ApiResponse<List<PMSSyncHistoryResponse>>), 200)]
        public async Task<IActionResult> GetSyncHistory(int integrationId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _pmsService.GetSyncHistoryAsync(integrationId, startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Senkronizasyon geçmişi detayını getir
        /// </summary>
        [HttpGet("sync-history/{syncHistoryId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSSyncHistoryResponse>), 200)]
        public async Task<IActionResult> GetSyncHistoryById(int syncHistoryId)
        {
            var result = await _pmsService.GetSyncHistoryByIdAsync(syncHistoryId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// PMS webhook'u işle
        /// </summary>
        [HttpPost("integrations/{integrationId}/webhook")]
        [AllowAnonymous] // Webhooks come from external PMS services
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> ProcessWebhook(
            int integrationId, 
            [FromBody] object payload, 
            [FromHeader(Name = "X-PMS-Signature")] string? signature = null,
            [FromHeader(Name = "X-PMS-Event-Type")] string? eventType = null)
        {
            if (payload == null)
                return BadRequest(new ApiResponse<bool> { Success = false, Message = "Webhook payload is required." });

            var payloadString = payload.ToString() ?? string.Empty;
            
            // Event type'ı payload'dan veya header'dan al
            if (string.IsNullOrEmpty(eventType))
            {
                // Payload'dan event type'ı çıkarmayı dene
                try
                {
                    var payloadJson = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(payloadString);
                    eventType = payloadJson?.ContainsKey("eventType") == true ? payloadJson["eventType"]?.ToString() :
                               payloadJson?.ContainsKey("event") == true ? payloadJson["event"]?.ToString() :
                               payloadJson?.ContainsKey("type") == true ? payloadJson["type"]?.ToString() :
                               "UNKNOWN";
                }
                catch
                {
                    eventType = "UNKNOWN";
                }
            }

            var result = await _webhookProcessor.ProcessWebhookAsync(integrationId, eventType ?? "UNKNOWN", payloadString, signature);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        #region Mock PMS Webhook Simulation (Development Only)

        /// <summary>
        /// [DEV ONLY] Mock check-in webhook simüle et
        /// </summary>
        [HttpPost("mock/simulate-checkin/{integrationId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSWebhookPayload>), 200)]
        public async Task<IActionResult> SimulateMockCheckIn(int integrationId, [FromServices] IMockPMSWebhookSimulator? simulator)
        {
            if (simulator == null)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Mock PMS Webhook Simulator is not available. Only available in Development environment." });

            var payload = await simulator.SimulateCheckInAsync(integrationId);
            return Ok(new ApiResponse<PMSWebhookPayload> { Success = true, Data = payload, Message = "Check-in webhook simulated successfully" });
        }

        /// <summary>
        /// [DEV ONLY] Mock check-out webhook simüle et
        /// </summary>
        [HttpPost("mock/simulate-checkout/{integrationId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSWebhookPayload>), 200)]
        public async Task<IActionResult> SimulateMockCheckOut(int integrationId, [FromServices] IMockPMSWebhookSimulator? simulator)
        {
            if (simulator == null)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Mock PMS Webhook Simulator is not available. Only available in Development environment." });

            var payload = await simulator.SimulateCheckOutAsync(integrationId);
            return Ok(new ApiResponse<PMSWebhookPayload> { Success = true, Data = payload, Message = "Check-out webhook simulated successfully" });
        }

        /// <summary>
        /// [DEV ONLY] Mock yeni rezervasyon webhook simüle et
        /// </summary>
        [HttpPost("mock/simulate-reservation/{integrationId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSWebhookPayload>), 200)]
        public async Task<IActionResult> SimulateMockNewReservation(int integrationId, [FromServices] IMockPMSWebhookSimulator? simulator)
        {
            if (simulator == null)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Mock PMS Webhook Simulator is not available. Only available in Development environment." });

            var payload = await simulator.SimulateNewReservationAsync(integrationId);
            return Ok(new ApiResponse<PMSWebhookPayload> { Success = true, Data = payload, Message = "New reservation webhook simulated successfully" });
        }

        /// <summary>
        /// [DEV ONLY] Mock rezervasyon iptali webhook simüle et
        /// </summary>
        [HttpPost("mock/simulate-cancellation/{integrationId}")]
        [ProducesResponseType(typeof(ApiResponse<PMSWebhookPayload>), 200)]
        public async Task<IActionResult> SimulateMockCancelReservation(int integrationId, [FromServices] IMockPMSWebhookSimulator? simulator)
        {
            if (simulator == null)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Mock PMS Webhook Simulator is not available. Only available in Development environment." });

            var payload = await simulator.SimulateCancelReservationAsync(integrationId);
            return Ok(new ApiResponse<PMSWebhookPayload> { Success = true, Data = payload, Message = "Cancellation webhook simulated successfully" });
        }

        #endregion
    }
}
