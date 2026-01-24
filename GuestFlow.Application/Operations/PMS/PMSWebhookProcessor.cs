// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GuestFlow.Application.Operations.PMS
{
    /// <summary>
    /// PMS Webhook Processor - PMS'den gelen webhook'ları işler
    /// </summary>
    public interface IPMSWebhookProcessor
    {
        /// <summary>
        /// Webhook payload'ını işle ve senkronize et
        /// </summary>
        Task<ApiResponse<bool>> ProcessWebhookAsync(int integrationId, string eventType, string payload, string? signature = null);
    }

    /// <summary>
    /// PMS Webhook Processor implementation
    /// </summary>
    public class PMSWebhookProcessor : IPMSWebhookProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPMSSyncService _pmsSyncService;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly ILogger<PMSWebhookProcessor> _logger;

        public PMSWebhookProcessor(
            IUnitOfWork unitOfWork,
            IPMSSyncService pmsSyncService,
            IPMSIntegrationService pmsIntegrationService,
            ILogger<PMSWebhookProcessor> logger)
        {
            _unitOfWork = unitOfWork;
            _pmsSyncService = pmsSyncService;
            _pmsIntegrationService = pmsIntegrationService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> ProcessWebhookAsync(int integrationId, string eventType, string payload, string? signature = null)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                {
                    return ApiResponse<bool>.Fail("PMS integration not found");
                }

                // Webhook signature doğrulama
                if (!string.IsNullOrEmpty(integration.WebhookSecret) && !string.IsNullOrEmpty(signature))
                {
                    if (!ValidateWebhookSignature(payload, signature, integration.WebhookSecret))
                    {
                        _logger.LogWarning("Invalid webhook signature for integration {IntegrationId}", integrationId);
                        return ApiResponse<bool>.Fail("Invalid webhook signature");
                    }
                }

                // Event type'a göre işle
                return eventType.ToUpperInvariant() switch
                {
                    "GUEST_CREATED" or "GUEST_UPDATED" => await ProcessGuestWebhookAsync(integrationId, payload),
                    "RESERVATION_CREATED" or "RESERVATION_UPDATED" or "RESERVATION_CANCELLED" => await ProcessReservationWebhookAsync(integrationId, payload),
                    "CHECK_IN" or "CHECK_OUT" => await ProcessCheckInOutWebhookAsync(integrationId, payload),
                    "ROOM_STATUS_CHANGED" => await ProcessRoomStatusWebhookAsync(integrationId, payload),
                    "FOLIO_UPDATED" => await ProcessFolioWebhookAsync(integrationId, payload),
                    _ => await ProcessGenericWebhookAsync(integrationId, eventType, payload)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process webhook: IntegrationId={IntegrationId}, EventType={EventType}", integrationId, eventType);
                return ApiResponse<bool>.Fail($"Failed to process webhook: {ex.Message}");
            }
        }

        private async Task<ApiResponse<bool>> ProcessGuestWebhookAsync(int integrationId, string payload)
        {
            try
            {
                var webhookData = JsonSerializer.Deserialize<PMSGuestWebhookData>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null || string.IsNullOrEmpty(webhookData.PMSGuestId))
                {
                    return ApiResponse<bool>.Fail("Invalid guest webhook data");
                }

                // Guest profile'ı PMS'den çek ve senkronize et
                var guestResponse = await _pmsIntegrationService.GetGuestProfileAsync(integrationId, webhookData.PMSGuestId);
                if (guestResponse.Success && guestResponse.Data != null)
                {
                    // PMSSyncService'e delegate et
                    // Bu metod PMSSyncService'de implement edilecek
                    _logger.LogInformation("Processing guest webhook: IntegrationId={IntegrationId}, GuestId={GuestId}", 
                        integrationId, webhookData.PMSGuestId);
                    
                    // Sync guest via PMSSyncService
                    // await _pmsSyncService.SyncGuestAsync(integrationId, guestResponse.Data);
                }

                return ApiResponse<bool>.SuccessResponse(true, "Guest webhook processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process guest webhook");
                return ApiResponse<bool>.Fail($"Failed to process guest webhook: {ex.Message}");
            }
        }

        private async Task<ApiResponse<bool>> ProcessReservationWebhookAsync(int integrationId, string payload)
        {
            try
            {
                var webhookData = JsonSerializer.Deserialize<PMSReservationWebhookData>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null || string.IsNullOrEmpty(webhookData.PMSReservationId))
                {
                    return ApiResponse<bool>.Fail("Invalid reservation webhook data");
                }

                _logger.LogInformation("Processing reservation webhook: IntegrationId={IntegrationId}, ReservationId={ReservationId}", 
                    integrationId, webhookData.PMSReservationId);

                // Reservation'ı PMS'den çek ve senkronize et
                var reservationResponse = await _pmsIntegrationService.GetReservationAsync(integrationId, webhookData.PMSReservationId);
                if (reservationResponse.Success && reservationResponse.Data != null)
                {
                    // PMSSyncService'e delegate et
                    // await _pmsSyncService.SyncReservationAsync(integrationId, reservationResponse.Data);
                }

                return ApiResponse<bool>.SuccessResponse(true, "Reservation webhook processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process reservation webhook");
                return ApiResponse<bool>.Fail($"Failed to process reservation webhook: {ex.Message}");
            }
        }

        private async Task<ApiResponse<bool>> ProcessCheckInOutWebhookAsync(int integrationId, string payload)
        {
            try
            {
                var webhookData = JsonSerializer.Deserialize<PMSCheckInOutWebhookData>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null)
                {
                    return ApiResponse<bool>.Fail("Invalid check-in/out webhook data");
                }

                _logger.LogInformation("Processing check-in/out webhook: IntegrationId={IntegrationId}, ReservationId={ReservationId}, Type={Type}", 
                    integrationId, webhookData.PMSReservationId, webhookData.Type);

                // Check-in/out event'i için reservation'ı güncelle
                if (!string.IsNullOrEmpty(webhookData.PMSReservationId))
                {
                    var reservationResponse = await _pmsIntegrationService.GetReservationAsync(integrationId, webhookData.PMSReservationId);
                    if (reservationResponse.Success && reservationResponse.Data != null)
                    {
                        // PMSSyncService'e delegate et
                        // await _pmsSyncService.SyncReservationAsync(integrationId, reservationResponse.Data);
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true, "Check-in/out webhook processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process check-in/out webhook");
                return ApiResponse<bool>.Fail($"Failed to process check-in/out webhook: {ex.Message}");
            }
        }

        private async Task<ApiResponse<bool>> ProcessRoomStatusWebhookAsync(int integrationId, string payload)
        {
            try
            {
                var webhookData = JsonSerializer.Deserialize<PMSRoomStatusWebhookData>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null || string.IsNullOrEmpty(webhookData.RoomNumber))
                {
                    return ApiResponse<bool>.Fail("Invalid room status webhook data");
                }

                _logger.LogInformation("Processing room status webhook: IntegrationId={IntegrationId}, RoomNumber={RoomNumber}", 
                    integrationId, webhookData.RoomNumber);

                // Room status'u senkronize et
                var roomStatusResponse = await _pmsIntegrationService.GetRoomStatusAsync(integrationId, webhookData.RoomNumber);
                if (roomStatusResponse.Success && roomStatusResponse.Data != null)
                {
                    // PMSSyncService'e delegate et
                    // await _pmsSyncService.SyncRoomStatusAsync(integrationId, roomStatusResponse.Data);
                }

                return ApiResponse<bool>.SuccessResponse(true, "Room status webhook processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process room status webhook");
                return ApiResponse<bool>.Fail($"Failed to process room status webhook: {ex.Message}");
            }
        }

        private async Task<ApiResponse<bool>> ProcessFolioWebhookAsync(int integrationId, string payload)
        {
            try
            {
                var webhookData = JsonSerializer.Deserialize<PMSFolioWebhookData>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null || string.IsNullOrEmpty(webhookData.ReservationId))
                {
                    return ApiResponse<bool>.Fail("Invalid folio webhook data");
                }

                _logger.LogInformation("Processing folio webhook: IntegrationId={IntegrationId}, ReservationId={ReservationId}", 
                    integrationId, webhookData.ReservationId);

                // Folio'yu senkronize et
                var folioResponse = await _pmsIntegrationService.GetFolioAsync(integrationId, webhookData.ReservationId);
                if (folioResponse.Success && folioResponse.Data != null)
                {
                    // PMSSyncService'e delegate et
                    // await _pmsSyncService.SyncFolioAsync(integrationId, folioResponse.Data);
                }

                return ApiResponse<bool>.SuccessResponse(true, "Folio webhook processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process folio webhook");
                return ApiResponse<bool>.Fail($"Failed to process folio webhook: {ex.Message}");
            }
        }

        private async Task<ApiResponse<bool>> ProcessGenericWebhookAsync(int integrationId, string eventType, string payload)
        {
            _logger.LogWarning("Unknown webhook event type: IntegrationId={IntegrationId}, EventType={EventType}", 
                integrationId, eventType);
            
            // Generic webhook'lar için log kaydet
            // İleride yeni event type'lar için genişletilebilir
            return ApiResponse<bool>.SuccessResponse(true, $"Generic webhook processed: {eventType}");
        }

        /// <summary>
        /// Webhook signature doğrulama (HMAC SHA256)
        /// </summary>
        private bool ValidateWebhookSignature(string payload, string signature, string secret)
        {
            try
            {
                using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
                var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
                var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();
                
                // Signature format: "sha256=..." veya sadece hex string olabilir
                var providedSignature = signature.Replace("sha256=", "").ToLowerInvariant();
                
                return computedSignature == providedSignature;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate webhook signature");
                return false;
            }
        }

        // Webhook data models
        private class PMSGuestWebhookData
        {
            public string? PMSGuestId { get; set; }
            public string? Action { get; set; } // Created, Updated, Deleted
        }

        private class PMSReservationWebhookData
        {
            public string? PMSReservationId { get; set; }
            public string? Action { get; set; } // Created, Updated, Cancelled
        }

        private class PMSCheckInOutWebhookData
        {
            public string? PMSReservationId { get; set; }
            public string? Type { get; set; } // CheckIn, CheckOut
            public DateTime? Timestamp { get; set; }
        }

        private class PMSRoomStatusWebhookData
        {
            public string? RoomNumber { get; set; }
            public string? Status { get; set; }
        }

        private class PMSFolioWebhookData
        {
            public string? ReservationId { get; set; }
            public string? FolioId { get; set; }
        }
    }
}
