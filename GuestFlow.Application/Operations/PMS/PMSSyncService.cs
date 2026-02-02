// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GuestFlow.Application.Operations.PMS
{
    /// <summary>
    /// PMS senkronizasyon servisi
    /// PMS verilerini GuestFlow sistemine senkronize eder
    /// </summary>
    public class PMSSyncService : IPMSSyncService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<RoomAssignmentEntity> _roomAssignmentRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<InvoiceItemEntity> _invoiceItemRepository;
        private readonly ILogger<PMSSyncService> _logger;

        public PMSSyncService(
            IUnitOfWork unitOfWork,
            IPMSIntegrationService pmsIntegrationService,
            IRepository<GuestEntity> guestRepository,
            IRepository<RoomAssignmentEntity> roomAssignmentRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<InvoiceItemEntity> invoiceItemRepository,
            ILogger<PMSSyncService> logger)
        {
            _unitOfWork = unitOfWork;
            _pmsIntegrationService = pmsIntegrationService;
            _guestRepository = guestRepository;
            _roomAssignmentRepository = roomAssignmentRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> SyncGuestsAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var syncHistory = await StartSyncHistoryAsync(integrationId, PMSSyncType.Guest, "Guest");
            
            try
            {
                var guestsResponse = await _pmsIntegrationService.GetGuestsAsync(integrationId, startDate, endDate);
                if (!guestsResponse.Success || guestsResponse.Data == null)
                {
                    await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Failed, 0, 0, 0, 
                        $"Failed to get guests: {guestsResponse.Message}");
                    return ApiResponse<bool>.Fail($"Failed to get guests: {guestsResponse.Message}");
                }

                var pmsGuests = guestsResponse.Data;
                int processed = 0, succeeded = 0, failed = 0;

                foreach (var pmsGuest in pmsGuests)
                {
                    try
                    {
                        processed++;
                        await SyncGuestAsync(integrationId, pmsGuest);
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogError(ex, "Failed to sync guest: {PMSGuestId}", pmsGuest.PMSGuestId);
                    }
                }

                await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Success, processed, succeeded, failed);
                return ApiResponse<bool>.SuccessResponse(true, $"Synced {succeeded} guests successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync guests for integration: {IntegrationId}", integrationId);
                await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Failed, 0, 0, 0, ex.Message);
                return ApiResponse<bool>.Fail($"Failed to sync guests: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SyncReservationsAsync(int integrationId, DateTime startDate, DateTime endDate)
        {
            var syncHistory = await StartSyncHistoryAsync(integrationId, PMSSyncType.Reservation, "Reservation");
            
            try
            {
                var reservationsResponse = await _pmsIntegrationService.GetReservationsAsync(integrationId, startDate, endDate);
                if (!reservationsResponse.Success || reservationsResponse.Data == null)
                {
                    await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Failed, 0, 0, 0, 
                        $"Failed to get reservations: {reservationsResponse.Message}");
                    return ApiResponse<bool>.Fail($"Failed to get reservations: {reservationsResponse.Message}");
                }

                var pmsReservations = reservationsResponse.Data;
                int processed = 0, succeeded = 0, failed = 0;

                foreach (var pmsReservation in pmsReservations)
                {
                    try
                    {
                        processed++;
                        await SyncReservationAsync(integrationId, pmsReservation);
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogError(ex, "Failed to sync reservation: {PMSReservationId}", pmsReservation.PMSReservationId);
                    }
                }

                await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Success, processed, succeeded, failed);
                return ApiResponse<bool>.SuccessResponse(true, $"Synced {succeeded} reservations successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync reservations for integration: {IntegrationId}", integrationId);
                await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Failed, 0, 0, 0, ex.Message);
                return ApiResponse<bool>.Fail($"Failed to sync reservations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SyncRoomsStatusAsync(int integrationId, DateTime? date = null)
        {
            var syncHistory = await StartSyncHistoryAsync(integrationId, PMSSyncType.Room, "Room");
            
            try
            {
                var roomsResponse = await _pmsIntegrationService.GetRoomsStatusAsync(integrationId, date);
                if (!roomsResponse.Success || roomsResponse.Data == null)
                {
                    await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Failed, 0, 0, 0, 
                        $"Failed to get rooms status: {roomsResponse.Message}");
                    return ApiResponse<bool>.Fail($"Failed to get rooms status: {roomsResponse.Message}");
                }

                var rooms = roomsResponse.Data;
                int processed = 0, succeeded = 0, failed = 0;

                foreach (var room in rooms)
                {
                    try
                    {
                        processed++;
                        await SyncRoomStatusAsync(integrationId, room);
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogError(ex, "Failed to sync room status: {RoomNumber}", room.RoomNumber);
                    }
                }

                await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Success, processed, succeeded, failed);
                return ApiResponse<bool>.SuccessResponse(true, $"Synced {succeeded} rooms status successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync rooms status for integration: {IntegrationId}", integrationId);
                await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Failed, 0, 0, 0, ex.Message);
                return ApiResponse<bool>.Fail($"Failed to sync rooms status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SyncFoliosAsync(int integrationId, DateTime startDate, DateTime endDate)
        {
            var syncHistory = await StartSyncHistoryAsync(integrationId, PMSSyncType.Folio, "Folio");
            
            try
            {
                var foliosResponse = await _pmsIntegrationService.GetFoliosAsync(integrationId, startDate, endDate);
                if (!foliosResponse.Success || foliosResponse.Data == null)
                {
                    await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Failed, 0, 0, 0, 
                        $"Failed to get folios: {foliosResponse.Message}");
                    return ApiResponse<bool>.Fail($"Failed to get folios: {foliosResponse.Message}");
                }

                var folios = foliosResponse.Data;
                int processed = 0, succeeded = 0, failed = 0;

                foreach (var folio in folios)
                {
                    try
                    {
                        processed++;
                        await SyncFolioAsync(integrationId, folio);
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogError(ex, "Failed to sync folio: {ReservationId}", folio.ReservationId);
                    }
                }

                await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Success, processed, succeeded, failed);
                return ApiResponse<bool>.SuccessResponse(true, $"Synced {succeeded} folios successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync folios for integration: {IntegrationId}", integrationId);
                await UpdateSyncHistoryAsync(syncHistory, PMSSyncStatus.Failed, 0, 0, 0, ex.Message);
                return ApiResponse<bool>.Fail($"Failed to sync folios: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> PerformFullSyncAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Starting full sync for integration: {IntegrationId}", integrationId);

                // Sync guests
                await SyncGuestsAsync(integrationId, startDate, endDate);

                // Sync reservations
                if (startDate.HasValue && endDate.HasValue)
                {
                    await SyncReservationsAsync(integrationId, startDate.Value, endDate.Value);
                }

                // Sync rooms status
                await SyncRoomsStatusAsync(integrationId, DateTime.UtcNow.Date);

                // Sync folios
                if (startDate.HasValue && endDate.HasValue)
                {
                    await SyncFoliosAsync(integrationId, startDate.Value, endDate.Value);
                }

                return ApiResponse<bool>.SuccessResponse(true, "Full sync completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform full sync for integration: {IntegrationId}", integrationId);
                return ApiResponse<bool>.Fail($"Full sync failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ProcessWebhookAsync(int integrationId, string payload, string? signature = null)
        {
            try
            {
                _logger.LogInformation("Processing webhook for integration: {IntegrationId}", integrationId);

                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("PMS integration not found");

                // Webhook signature doğrulama
                if (!string.IsNullOrEmpty(integration.WebhookSecret) && !string.IsNullOrEmpty(signature))
                {
                    if (!ValidateWebhookSignature(payload, signature, integration.WebhookSecret))
                    {
                        _logger.LogWarning("Invalid webhook signature for integration: {IntegrationId}", integrationId);
                        return ApiResponse<bool>.Fail("Invalid webhook signature");
                    }
                }

                // Webhook payload'ını parse et
                var webhookData = JsonSerializer.Deserialize<Dictionary<string, object>>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null)
                    return ApiResponse<bool>.Fail("Invalid webhook payload");

                // Webhook event type'ına göre işlem yap
                var eventType = webhookData.ContainsKey("eventType") ? webhookData["eventType"]?.ToString() : 
                               webhookData.ContainsKey("event") ? webhookData["event"]?.ToString() : 
                               webhookData.ContainsKey("type") ? webhookData["type"]?.ToString() : null;

                if (string.IsNullOrEmpty(eventType))
                {
                    _logger.LogWarning("Webhook event type not found for integration: {IntegrationId}", integrationId);
                    return ApiResponse<bool>.Fail("Webhook event type not found");
                }

                // Event type'a göre sync işlemi yap
                switch (eventType.ToUpperInvariant())
                {
                    case "GUEST_CREATED":
                    case "GUEST_UPDATED":
                        await HandleGuestWebhookAsync(integrationId, webhookData);
                        break;

                    case "RESERVATION_CREATED":
                    case "RESERVATION_UPDATED":
                    case "RESERVATION_CANCELLED":
                        await HandleReservationWebhookAsync(integrationId, webhookData);
                        break;

                    case "ROOM_STATUS_CHANGED":
                    case "ROOM_ASSIGNED":
                        await HandleRoomWebhookAsync(integrationId, webhookData);
                        break;

                    case "FOLIO_UPDATED":
                    case "FOLIO_CLOSED":
                        await HandleFolioWebhookAsync(integrationId, webhookData);
                        break;

                    default:
                        _logger.LogWarning("Unknown webhook event type: {EventType} for integration: {IntegrationId}", eventType, integrationId);
                        break;
                }

                return ApiResponse<bool>.SuccessResponse(true, "Webhook processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process webhook for integration: {IntegrationId}", integrationId);
                return ApiResponse<bool>.Fail($"Webhook processing failed: {ex.Message}");
            }
        }

        private bool ValidateWebhookSignature(string payload, string signature, string secret)
        {
            try
            {
                // HMAC SHA256 ile signature doğrulama
                using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
                var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
                var computedSignature = Convert.ToHexString(hashBytes).ToLowerInvariant();
                
                // Signature format'ı provider'a göre değişebilir (hex, base64, vb.)
                return computedSignature == signature.ToLowerInvariant() || 
                       Convert.ToBase64String(hashBytes) == signature;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate webhook signature");
                return false;
            }
        }

        private async Task HandleGuestWebhookAsync(int integrationId, Dictionary<string, object> webhookData)
        {
            try
            {
                // Webhook'dan guest bilgilerini çıkar
                var pmsGuestId = webhookData.ContainsKey("guestId") ? webhookData["guestId"]?.ToString() :
                                webhookData.ContainsKey("guest_id") ? webhookData["guest_id"]?.ToString() : null;

                if (string.IsNullOrEmpty(pmsGuestId))
                {
                    _logger.LogWarning("Guest ID not found in webhook payload");
                    return;
                }

                // PMS'den güncel guest bilgisini çek
                var guestResponse = await _pmsIntegrationService.GetGuestProfileAsync(integrationId, pmsGuestId);
                if (guestResponse.Success && guestResponse.Data != null)
                {
                    await SyncGuestAsync(integrationId, guestResponse.Data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle guest webhook");
                throw;
            }
        }

        private async Task HandleReservationWebhookAsync(int integrationId, Dictionary<string, object> webhookData)
        {
            try
            {
                var pmsReservationId = webhookData.ContainsKey("reservationId") ? webhookData["reservationId"]?.ToString() :
                                       webhookData.ContainsKey("reservation_id") ? webhookData["reservation_id"]?.ToString() : null;

                if (string.IsNullOrEmpty(pmsReservationId))
                {
                    _logger.LogWarning("Reservation ID not found in webhook payload");
                    return;
                }

                // PMS'den güncel reservation bilgisini çek
                var reservationResponse = await _pmsIntegrationService.GetReservationAsync(integrationId, pmsReservationId);
                if (reservationResponse.Success && reservationResponse.Data != null)
                {
                    await SyncReservationAsync(integrationId, reservationResponse.Data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle reservation webhook");
                throw;
            }
        }

        private async Task HandleRoomWebhookAsync(int integrationId, Dictionary<string, object> webhookData)
        {
            try
            {
                var roomNumber = webhookData.ContainsKey("roomNumber") ? webhookData["roomNumber"]?.ToString() :
                                webhookData.ContainsKey("room_number") ? webhookData["room_number"]?.ToString() : null;

                if (string.IsNullOrEmpty(roomNumber))
                {
                    _logger.LogWarning("Room number not found in webhook payload");
                    return;
                }

                // PMS'den güncel room status bilgisini çek
                var roomResponse = await _pmsIntegrationService.GetRoomStatusAsync(integrationId, roomNumber);
                if (roomResponse.Success && roomResponse.Data != null)
                {
                    await SyncRoomStatusAsync(integrationId, roomResponse.Data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle room webhook");
                throw;
            }
        }

        private async Task HandleFolioWebhookAsync(int integrationId, Dictionary<string, object> webhookData)
        {
            try
            {
                var reservationId = webhookData.ContainsKey("reservationId") ? webhookData["reservationId"]?.ToString() :
                                    webhookData.ContainsKey("reservation_id") ? webhookData["reservation_id"]?.ToString() : null;

                if (string.IsNullOrEmpty(reservationId))
                {
                    _logger.LogWarning("Reservation ID not found in folio webhook payload");
                    return;
                }

                // PMS'den güncel folio bilgisini çek
                var folioResponse = await _pmsIntegrationService.GetFolioAsync(integrationId, reservationId);
                if (folioResponse.Success && folioResponse.Data != null)
                {
                    // Folio sync logic
                    // TODO: Implement folio sync
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle folio webhook");
                throw;
            }
        }

        public async Task ProcessPollingSyncAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var activeIntegrations = await _unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted && i.SyncMode == PMSSyncMode.Polling)
                    .ToListAsync(cancellationToken);

                foreach (var integration in activeIntegrations)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        // Son sync'ten bu yana geçen süreyi kontrol et
                        var timeSinceLastSync = DateTime.UtcNow - (integration.LastSyncDate ?? DateTime.MinValue);
                        var pollingInterval = TimeSpan.FromMinutes(integration.PollingIntervalMinutes);

                        if (timeSinceLastSync >= pollingInterval)
                        {
                            _logger.LogInformation("Starting polling sync for integration: {IntegrationId}", integration.Id);
                            
                            // Son 24 saat için sync yap
                            var endDate = DateTime.UtcNow;
                            var startDate = endDate.AddDays(-1);
                            
                            await PerformFullSyncAsync(integration.Id, startDate, endDate);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process polling sync for integration: {IntegrationId}", integration.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process polling sync");
            }
        }

        #region Private Helper Methods

        private async Task<PMSSyncHistory> StartSyncHistoryAsync(int integrationId, PMSSyncType syncType, string entityType)
        {
            var syncHistory = new PMSSyncHistory
            {
                PMSIntegrationId = integrationId,
                SyncType = syncType,
                EntityType = entityType,
                Status = PMSSyncStatus.InProgress,
                SyncStartTime = DateTime.UtcNow,
                RecordsProcessed = 0,
                RecordsSucceeded = 0,
                RecordsFailed = 0
            };

            await _unitOfWork.PMSSyncHistories.AddAsync(syncHistory);
            await _unitOfWork.CommitAsync();

            return syncHistory;
        }

        private async Task UpdateSyncHistoryAsync(
            PMSSyncHistory syncHistory,
            PMSSyncStatus status,
            int processed,
            int succeeded,
            int failed,
            string? errorMessage = null)
        {
            syncHistory.Status = status;
            syncHistory.SyncEndTime = DateTime.UtcNow;
            syncHistory.RecordsProcessed = processed;
            syncHistory.RecordsSucceeded = succeeded;
            syncHistory.RecordsFailed = failed;
            syncHistory.ErrorMessage = errorMessage;

            _unitOfWork.PMSSyncHistories.Update(syncHistory);
            await _unitOfWork.CommitAsync();
        }

        public async Task<ApiResponse<bool>> SyncGuestAsync(int integrationId, PMSGuestProfile pmsGuest)
        {
            try
            {
                // Mapping kontrolü
                var existingMapping = await _unitOfWork.PMSGuestMappings
                    .GetAll(m => m.PMSIntegrationId == integrationId && m.PMSGuestId == pmsGuest.PMSGuestId)
                    .FirstOrDefaultAsync();

                GuestEntity? guestEntity = null;
                var isNewGuest = false;

                if (existingMapping != null && existingMapping.GuestFlowGuestId > 0)
                {
                    // Mevcut misafiri güncelle
                    guestEntity = await _guestRepository.GetByIdAsync(existingMapping.GuestFlowGuestId);
                }

                if (guestEntity == null)
                {
                    // Yeni misafir oluştur
                    isNewGuest = true;
                    var guestCode = await GenerateGuestCodeAsync();
                    guestEntity = new GuestEntity
                    {
                        FullName = pmsGuest.FullName ?? "Unknown",
                        Email = pmsGuest.Email ?? string.Empty,
                        PhoneNumber = pmsGuest.PhoneNumber ?? string.Empty,
                        Nationality = pmsGuest.Nationality ?? string.Empty,
                        GuestCode = guestCode,
                        IsSpecialGuest = pmsGuest.IsVIP
                    };

                    await _guestRepository.AddAsync(guestEntity);
                    await _unitOfWork.CommitAsync();

                    // Mapping oluştur
                    if (existingMapping == null)
                    {
                        existingMapping = new PMSGuestMapping
                        {
                            PMSIntegrationId = integrationId,
                            PMSGuestId = pmsGuest.PMSGuestId,
                            GuestFlowGuestId = guestEntity.Id,
                            SyncStatus = "Synced",
                            LastSyncedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.PMSGuestMappings.AddAsync(existingMapping);
                    }
                    else
                    {
                        existingMapping.GuestFlowGuestId = guestEntity.Id;
                        existingMapping.SyncStatus = "Synced";
                        existingMapping.LastSyncedAt = DateTime.UtcNow;
                        _unitOfWork.PMSGuestMappings.Update(existingMapping);
                    }
                }
                else
                {
                    // Mevcut misafiri güncelle
                    guestEntity.FullName = pmsGuest.FullName ?? guestEntity.FullName;
                    guestEntity.Email = pmsGuest.Email ?? guestEntity.Email;
                    guestEntity.PhoneNumber = pmsGuest.PhoneNumber ?? guestEntity.PhoneNumber;
                    guestEntity.Nationality = pmsGuest.Nationality ?? guestEntity.Nationality;
                    guestEntity.IsSpecialGuest = pmsGuest.IsVIP; // VIP durumu güncelle
                    guestEntity.MarkAsUpdated();
                    _guestRepository.Update(guestEntity);
                }

                // RoomNumber, CheckInDate, CheckOutDate sync (PMS'den gelen güncel bilgiler)
                if (!string.IsNullOrEmpty(pmsGuest.RoomNumber))
                {
                    guestEntity.RoomNumber = pmsGuest.RoomNumber;
                }
                if (pmsGuest.CheckInDate.HasValue)
                {
                    guestEntity.CheckInDate = pmsGuest.CheckInDate;
                }
                if (pmsGuest.CheckOutDate.HasValue)
                {
                    guestEntity.CheckOutDate = pmsGuest.CheckOutDate;
                }

                // Preferences sync
                await SyncGuestPreferencesAsync(guestEntity.Id, pmsGuest, integrationId);

                await _unitOfWork.CommitAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Guest synced successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync guest: {PMSGuestId}", pmsGuest.PMSGuestId);
                return ApiResponse<bool>.Fail($"Failed to sync guest: {ex.Message}");
            }
        }

        private async Task SyncGuestPreferencesAsync(int guestId, PMSGuestProfile pmsGuest, int integrationId)
        {
            try
            {
                var existingPreferences = await _unitOfWork.GuestPreferences
                    .GetAll(p => p.GuestId == guestId)
                    .FirstOrDefaultAsync();

                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                var source = integration != null ? $"{integration.ProviderName} PMS" : "PMS";

                if (existingPreferences == null)
                {
                    // Yeni preferences oluştur
                    existingPreferences = new GuestPreferencesEntity
                    {
                        GuestId = guestId,
                        Source = source
                    };
                    await _unitOfWork.GuestPreferences.AddAsync(existingPreferences);
                }
                else
                {
                    existingPreferences.MarkAsUpdated();
                }

                // SpecialRequests'i Notes veya RoomSpecialRequests olarak kaydet
                if (!string.IsNullOrEmpty(pmsGuest.SpecialRequests))
                {
                    if (string.IsNullOrEmpty(existingPreferences.RoomSpecialRequests))
                    {
                        existingPreferences.RoomSpecialRequests = pmsGuest.SpecialRequests;
                    }
                    else if (string.IsNullOrEmpty(existingPreferences.Notes))
                    {
                        existingPreferences.Notes = pmsGuest.SpecialRequests;
                    }
                }

                // Preferences JSON'ını parse et (eğer varsa)
                if (!string.IsNullOrEmpty(pmsGuest.Preferences))
                {
                    try
                    {
                        var preferencesJson = JsonSerializer.Deserialize<Dictionary<string, object>>(pmsGuest.Preferences, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (preferencesJson != null)
                        {
                            // Oda tercihleri
                            if (preferencesJson.ContainsKey("preferredRoomType") && preferencesJson["preferredRoomType"] != null)
                                existingPreferences.PreferredRoomType = preferencesJson["preferredRoomType"].ToString();
                            
                            if (preferencesJson.ContainsKey("roomSpecialRequests") && preferencesJson["roomSpecialRequests"] != null)
                                existingPreferences.RoomSpecialRequests = preferencesJson["roomSpecialRequests"].ToString();
                            
                            if (preferencesJson.ContainsKey("bedPreference") && preferencesJson["bedPreference"] != null)
                                existingPreferences.BedPreference = preferencesJson["bedPreference"].ToString();
                            
                            if (preferencesJson.ContainsKey("smokingPreference") && preferencesJson["smokingPreference"] != null)
                                existingPreferences.SmokingPreference = preferencesJson["smokingPreference"].ToString();

                            // Yemek tercihleri
                            if (preferencesJson.ContainsKey("dietaryPreferences") && preferencesJson["dietaryPreferences"] != null)
                                existingPreferences.DietaryPreferences = preferencesJson["dietaryPreferences"].ToString();
                            
                            if (preferencesJson.ContainsKey("foodAllergies") && preferencesJson["foodAllergies"] != null)
                                existingPreferences.FoodAllergies = preferencesJson["foodAllergies"].ToString();
                            
                            if (preferencesJson.ContainsKey("specialFoodRequests") && preferencesJson["specialFoodRequests"] != null)
                                existingPreferences.SpecialFoodRequests = preferencesJson["specialFoodRequests"].ToString();

                            // Aktivite tercihleri
                            if (preferencesJson.ContainsKey("activityPreferences") && preferencesJson["activityPreferences"] != null)
                                existingPreferences.ActivityPreferences = preferencesJson["activityPreferences"].ToString();
                            
                            if (preferencesJson.ContainsKey("interests") && preferencesJson["interests"] != null)
                                existingPreferences.Interests = preferencesJson["interests"].ToString();

                            // İletişim tercihleri
                            if (preferencesJson.ContainsKey("prefersEmail") && preferencesJson["prefersEmail"] != null)
                                existingPreferences.PrefersEmail = Convert.ToBoolean(preferencesJson["prefersEmail"]);
                            
                            if (preferencesJson.ContainsKey("prefersSMS") && preferencesJson["prefersSMS"] != null)
                                existingPreferences.PrefersSMS = Convert.ToBoolean(preferencesJson["prefersSMS"]);
                            
                            if (preferencesJson.ContainsKey("prefersWhatsApp") && preferencesJson["prefersWhatsApp"] != null)
                                existingPreferences.PrefersWhatsApp = Convert.ToBoolean(preferencesJson["prefersWhatsApp"]);
                            
                            if (preferencesJson.ContainsKey("prefersPhone") && preferencesJson["prefersPhone"] != null)
                                existingPreferences.PrefersPhone = Convert.ToBoolean(preferencesJson["prefersPhone"]);
                            
                            if (preferencesJson.ContainsKey("preferredLanguage") && preferencesJson["preferredLanguage"] != null)
                                existingPreferences.PreferredLanguage = preferencesJson["preferredLanguage"].ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse preferences JSON for guest {GuestId}", guestId);
                    }
                }

                if (existingPreferences.Id > 0)
                {
                    _unitOfWork.GuestPreferences.Update(existingPreferences);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync guest preferences for guest {GuestId}", guestId);
                // Preferences sync hatası misafir sync'ini engellemez
            }
        }

        public async Task<ApiResponse<bool>> SyncReservationAsync(int integrationId, PMSReservation pmsReservation)
        {
            try
            {
                // Mapping kontrolü
                var existingMapping = await _unitOfWork.PMSReservationMappings
                    .GetAll(m => m.PMSIntegrationId == integrationId && m.PMSReservationId == pmsReservation.PMSReservationId)
                    .FirstOrDefaultAsync();

                // Misafir mapping'ini bul - PMSReservation'dan PMSGuestId ile
                var guestMapping = await _unitOfWork.PMSGuestMappings
                    .GetAll(m => m.PMSIntegrationId == integrationId && m.PMSGuestId == pmsReservation.PMSGuestId)
                    .FirstOrDefaultAsync();

                if (guestMapping == null || guestMapping.GuestFlowGuestId <= 0)
                {
                    _logger.LogWarning("Guest mapping not found for PMS reservation: {PMSReservationId}, PMSGuestId: {PMSGuestId}", 
                        pmsReservation.PMSReservationId, pmsReservation.PMSGuestId);
                    // Guest mapping yoksa, önce guest'i sync et
                    var guestResponse = await _pmsIntegrationService.GetGuestProfileAsync(integrationId, pmsReservation.PMSGuestId);
                    if (guestResponse.Success && guestResponse.Data != null)
                    {
                        var syncGuestResult = await SyncGuestAsync(integrationId, guestResponse.Data);
                        if (!syncGuestResult.Success)
                        {
                             return ApiResponse<bool>.Fail($"Failed to sync guest for reservation: {syncGuestResult.Message}");
                        }

                        // Guest sync sonrası mapping'i tekrar al
                        guestMapping = await _unitOfWork.PMSGuestMappings
                            .GetAll(m => m.PMSIntegrationId == integrationId && m.PMSGuestId == pmsReservation.PMSGuestId)
                            .FirstOrDefaultAsync();
                    }

                    if (guestMapping == null || guestMapping.GuestFlowGuestId <= 0)
                    {
                        _logger.LogError("Failed to sync guest for PMS reservation: {PMSReservationId}", pmsReservation.PMSReservationId);
                        return ApiResponse<bool>.Fail("Failed to resolve guest for reservation");
                    }
                }

                var guestId = guestMapping.GuestFlowGuestId;
                var guestEntity = await _guestRepository.GetByIdAsync(guestId);
                if (guestEntity == null)
                {
                    _logger.LogError("Guest entity not found for ID: {GuestId}", guestId);
                    return ApiResponse<bool>.Fail("Guest entity not found");
                }

                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                var source = integration != null ? $"{integration.ProviderName} PMS" : "PMS";

                // GuestEntity'yi güncelle (CheckInDate, CheckOutDate, RoomNumber)
                guestEntity.CheckInDate = pmsReservation.CheckInDate;
                guestEntity.CheckOutDate = pmsReservation.CheckOutDate;
                if (!string.IsNullOrEmpty(pmsReservation.RoomNumber))
                {
                    guestEntity.RoomNumber = pmsReservation.RoomNumber;
                }
                guestEntity.MarkAsUpdated();
                _guestRepository.Update(guestEntity);

                // Status'a göre RoomAssignment oluştur/güncelle
                var status = pmsReservation.Status.ToUpperInvariant();
                var checkInDate = pmsReservation.CheckInDate.Date;
                var checkOutDate = pmsReservation.CheckOutDate.Date;

                if (status == "CHECKEDIN" || status == "CONFIRMED")
                {
                    // Check-in: RoomAssignment oluştur veya güncelle
                    await SyncRoomAssignmentForReservationAsync(guestId, pmsReservation, integrationId, source);
                }
                else if (status == "CHECKEDOUT")
                {
                    // Check-out: Mevcut RoomAssignment'ı kapat
                    var activeAssignment = await _roomAssignmentRepository
                        .GetAll(ra => ra.GuestId == guestId &&
                                     ra.RoomNumber == pmsReservation.RoomNumber &&
                                     (!ra.EndDate.HasValue || ra.EndDate >= checkInDate))
                        .OrderByDescending(ra => ra.StartDate)
                        .FirstOrDefaultAsync();

                    if (activeAssignment != null)
                    {
                        activeAssignment.EndDate = checkOutDate;
                        activeAssignment.MarkAsUpdated();
                        _roomAssignmentRepository.Update(activeAssignment);
                    }
                }
                else if (status == "CANCELLED")
                {
                    // İptal: Mevcut RoomAssignment'ı iptal et
                    var activeAssignment = await _roomAssignmentRepository
                        .GetAll(ra => ra.GuestId == guestId &&
                                     ra.RoomNumber == pmsReservation.RoomNumber &&
                                     (!ra.EndDate.HasValue || ra.EndDate >= checkInDate))
                        .OrderByDescending(ra => ra.StartDate)
                        .FirstOrDefaultAsync();

                    if (activeAssignment != null)
                    {
                        activeAssignment.EndDate = DateTime.UtcNow.Date; // İptal tarihi
                        activeAssignment.Notes = $"{activeAssignment.Notes ?? string.Empty} [CANCELLED from {source}]".Trim();
                        activeAssignment.MarkAsUpdated();
                        _roomAssignmentRepository.Update(activeAssignment);
                    }
                }

                // Reservation mapping oluştur/güncelle
                if (existingMapping == null)
                {
                    existingMapping = new PMSReservationMapping
                    {
                        PMSIntegrationId = integrationId,
                        PMSReservationId = pmsReservation.PMSReservationId,
                        GuestFlowReservationId = null, // PMS rezervasyonları için GuestFlow ReservationEntity kullanmıyoruz
                        SyncStatus = "Synced",
                        LastSyncedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.PMSReservationMappings.AddAsync(existingMapping);
                }
                else
                {
                    existingMapping.SyncStatus = "Synced";
                    existingMapping.LastSyncedAt = DateTime.UtcNow;
                    _unitOfWork.PMSReservationMappings.Update(existingMapping);
                }

                await _unitOfWork.CommitAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Reservation synced successfully");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to sync reservation: {PMSReservationId}", pmsReservation.PMSReservationId);
                 return ApiResponse<bool>.Fail($"Failed to sync reservation: {ex.Message}");
            }
        }

        private async Task SyncRoomAssignmentForReservationAsync(int guestId, PMSReservation pmsReservation, int integrationId, string source)
        {
            if (string.IsNullOrEmpty(pmsReservation.RoomNumber))
                return;

            var checkInDate = pmsReservation.CheckInDate.Date;
            var checkOutDate = pmsReservation.CheckOutDate.Date;

            // Mevcut active assignment'ı kontrol et
            var existingAssignment = await _roomAssignmentRepository
                .GetAll(ra => ra.GuestId == guestId &&
                             ra.RoomNumber == pmsReservation.RoomNumber &&
                             ra.StartDate <= checkOutDate &&
                             (!ra.EndDate.HasValue || ra.EndDate >= checkInDate))
                .OrderByDescending(ra => ra.StartDate)
                .FirstOrDefaultAsync();

            if (existingAssignment == null)
            {
                // Yeni RoomAssignment oluştur
                var newAssignment = new RoomAssignmentEntity
                {
                    GuestId = guestId,
                    RoomNumber = pmsReservation.RoomNumber,
                    StartDate = checkInDate,
                    EndDate = checkOutDate,
                    Source = source,
                    Notes = $"PMS Reservation: {pmsReservation.PMSReservationId} | Room Type: {pmsReservation.RoomType ?? "N/A"} | Guests: {pmsReservation.GuestCount}"
                };
                await _roomAssignmentRepository.AddAsync(newAssignment);
            }
            else
            {
                // Mevcut assignment'ı güncelle (tarihler değişmiş olabilir)
                if (existingAssignment.StartDate != checkInDate || existingAssignment.EndDate != checkOutDate)
                {
                    // Eğer tarih aralığı değiştiyse, eski assignment'ı kapat ve yeni oluştur
                    if (existingAssignment.StartDate < checkInDate)
                    {
                        existingAssignment.EndDate = checkInDate.AddDays(-1);
                        existingAssignment.MarkAsUpdated();
                        _roomAssignmentRepository.Update(existingAssignment);

                        var newAssignment = new RoomAssignmentEntity
                        {
                            GuestId = guestId,
                            RoomNumber = pmsReservation.RoomNumber,
                            StartDate = checkInDate,
                            EndDate = checkOutDate,
                            Source = source,
                            Notes = $"PMS Reservation: {pmsReservation.PMSReservationId} | Room Type: {pmsReservation.RoomType ?? "N/A"} | Guests: {pmsReservation.GuestCount}"
                        };
                        await _roomAssignmentRepository.AddAsync(newAssignment);
                    }
                    else
                    {
                        existingAssignment.StartDate = checkInDate;
                        existingAssignment.EndDate = checkOutDate;
                        existingAssignment.MarkAsUpdated();
                        _roomAssignmentRepository.Update(existingAssignment);
                    }
                }
            }
        }

        public async Task<ApiResponse<bool>> SyncRoomStatusAsync(int integrationId, PMSRoomStatus roomStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(roomStatus.RoomNumber))
                    return ApiResponse<bool>.Fail("Room number is empty");

                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null) return ApiResponse<bool>.Fail("Integration not found");

                var source = $"{integration.ProviderName} PMS";
                var status = roomStatus.Status.ToUpperInvariant();

                // Status'a göre işlem yap
                if (status == "AVAILABLE" || status == "VACANT")
                {
                    // Oda boş - mevcut assignment'ları kapat
                    await CloseRoomAssignmentsForRoomAsync(roomStatus.RoomNumber, source);
                    return ApiResponse<bool>.SuccessResponse(true, "Room status synced (Vacant)");
                }

                // Occupied, OutOfOrder, Maintenance durumları için guest bilgisi gerekli
                if (string.IsNullOrEmpty(roomStatus.PMSGuestId))
                {
                    // Guest bilgisi yoksa sadece status'u logla (OutOfOrder, Maintenance durumları için)
                    if (status == "OUTOFORDER" || status == "MAINTENANCE")
                    {
                        _logger.LogInformation("Room {RoomNumber} status updated to {Status} from {Source}", 
                            roomStatus.RoomNumber, status, source);
                        return ApiResponse<bool>.SuccessResponse(true, $"Room status synced ({status})");
                    }
                    _logger.LogWarning("PMSGuestId not found for room status: {RoomNumber}, Status: {Status}", 
                        roomStatus.RoomNumber, status);
                    return ApiResponse<bool>.Fail("PMSGuestId required for occupied status");
                }

                // Guest mapping'ini bul
                var guestMapping = await _unitOfWork.PMSGuestMappings
                    .GetAll(m => m.PMSIntegrationId == integrationId && m.PMSGuestId == roomStatus.PMSGuestId)
                    .FirstOrDefaultAsync();

                if (guestMapping?.GuestFlowGuestId == null || guestMapping.GuestFlowGuestId <= 0)
                {
                    _logger.LogWarning("Guest mapping not found for PMS guest ID: {PMSGuestId}, Room: {RoomNumber}", 
                        roomStatus.PMSGuestId, roomStatus.RoomNumber);
                    return ApiResponse<bool>.Fail("Guest mapping not found");
                }

                var guestId = guestMapping.GuestFlowGuestId;
                var guestEntity = await _guestRepository.GetByIdAsync(guestId);
                if (guestEntity == null)
                {
                    _logger.LogError("Guest entity not found for ID: {GuestId}", guestId);
                    return ApiResponse<bool>.Fail("Guest entity not found");
                }

                // GuestEntity'yi güncelle (RoomNumber)
                if (status == "OCCUPIED" || status == "CHECKEDIN")
                {
                    guestEntity.RoomNumber = roomStatus.RoomNumber;
                    if (roomStatus.CheckInDate.HasValue)
                    {
                        guestEntity.CheckInDate = roomStatus.CheckInDate;
                    }
                    if (roomStatus.CheckOutDate.HasValue)
                    {
                        guestEntity.CheckOutDate = roomStatus.CheckOutDate;
                    }
                    guestEntity.MarkAsUpdated();
                    _guestRepository.Update(guestEntity);
                }

                // Mevcut room assignment'ı kontrol et
                var existingAssignment = await _roomAssignmentRepository
                    .GetAll(ra => ra.GuestId == guestId && 
                                 ra.RoomNumber == roomStatus.RoomNumber &&
                                 (!ra.EndDate.HasValue || ra.EndDate >= DateTime.UtcNow.Date))
                    .OrderByDescending(ra => ra.StartDate)
                    .FirstOrDefaultAsync();

                var checkInDate = roomStatus.CheckInDate ?? DateTime.UtcNow.Date;
                var checkOutDate = roomStatus.CheckOutDate;

                if (status == "OCCUPIED" || status == "CHECKEDIN")
                {
                    // Oda dolu - RoomAssignment oluştur veya güncelle
                    if (existingAssignment == null)
                    {
                        // Yeni room assignment oluştur
                        var newAssignment = new RoomAssignmentEntity
                        {
                            GuestId = guestId,
                            RoomNumber = roomStatus.RoomNumber,
                            StartDate = checkInDate,
                            EndDate = checkOutDate,
                            Source = source,
                            Notes = $"Synced from {source} - Status: {roomStatus.Status} | Room Type: {roomStatus.RoomType ?? "N/A"}"
                        };

                        await _roomAssignmentRepository.AddAsync(newAssignment);
                    }
                    else
                    {
                        // Mevcut assignment'ı güncelle
                        if (existingAssignment.StartDate != checkInDate || existingAssignment.EndDate != checkOutDate)
                        {
                            // Eğer tarih değiştiyse, eski assignment'ı kapat ve yeni oluştur
                            if (existingAssignment.StartDate < checkInDate)
                            {
                                existingAssignment.EndDate = checkInDate.AddDays(-1);
                                existingAssignment.MarkAsUpdated();
                                _roomAssignmentRepository.Update(existingAssignment);

                                var newAssignment = new RoomAssignmentEntity
                                {
                                    GuestId = guestId,
                                    RoomNumber = roomStatus.RoomNumber,
                                    StartDate = checkInDate,
                                    EndDate = checkOutDate,
                                    Source = source,
                                    Notes = $"Synced from {source} - Status: {roomStatus.Status} | Room Type: {roomStatus.RoomType ?? "N/A"}"
                                };
                                await _roomAssignmentRepository.AddAsync(newAssignment);
                            }
                            else
                            {
                                existingAssignment.StartDate = checkInDate;
                                existingAssignment.EndDate = checkOutDate;
                                existingAssignment.MarkAsUpdated();
                                _roomAssignmentRepository.Update(existingAssignment);
                            }
                        }
                    }
                }
                else if (status == "CHECKEDOUT" || status == "VACANT")
                {
                    // Check-out - mevcut assignment'ı kapat
                    if (existingAssignment != null)
                    {
                        existingAssignment.EndDate = checkOutDate ?? DateTime.UtcNow.Date;
                        existingAssignment.MarkAsUpdated();
                        _roomAssignmentRepository.Update(existingAssignment);
                    }
                }

                await _unitOfWork.CommitAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Room status synced successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync room status for room: {RoomNumber}", roomStatus.RoomNumber);
                return ApiResponse<bool>.Fail($"Failed to sync room status: {ex.Message}");
            }
        }

        private async Task CloseRoomAssignmentsForRoomAsync(string roomNumber, string source)
        {
            // Belirli bir oda için tüm aktif assignment'ları kapat
            var activeAssignments = await _roomAssignmentRepository
                .GetAll(ra => ra.RoomNumber == roomNumber &&
                             (!ra.EndDate.HasValue || ra.EndDate >= DateTime.UtcNow.Date))
                .ToListAsync();

            foreach (var assignment in activeAssignments)
            {
                assignment.EndDate = DateTime.UtcNow.Date;
                assignment.Notes = $"{assignment.Notes ?? string.Empty} [Room status changed to Available from {source}]".Trim();
                assignment.MarkAsUpdated();
                _roomAssignmentRepository.Update(assignment);
            }
        }

        public async Task<ApiResponse<bool>> SyncFolioAsync(int integrationId, PMSFolio pmsFolio)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null) return ApiResponse<bool>.Fail("Integration not found");

                var source = $"{integration.ProviderName} PMS";

                // Reservation'dan guest bilgisini al
                var reservationResponse = await _pmsIntegrationService.GetReservationAsync(integrationId, pmsFolio.ReservationId);
                if (!reservationResponse.Success || reservationResponse.Data == null)
                {
                    _logger.LogWarning("Reservation not found for folio: {ReservationId}", pmsFolio.ReservationId);
                    return ApiResponse<bool>.Fail($"Reservation not found for folio: {pmsFolio.ReservationId}");
                }

                var reservation = reservationResponse.Data;

                // Guest mapping'ini bul
                var guestMapping = await _unitOfWork.PMSGuestMappings
                    .GetAll(m => m.PMSIntegrationId == integrationId && m.PMSGuestId == reservation.PMSGuestId)
                    .FirstOrDefaultAsync();

                if (guestMapping?.GuestFlowGuestId == null || guestMapping.GuestFlowGuestId <= 0)
                {
                    _logger.LogWarning("Guest mapping not found for PMS guest ID: {PMSGuestId}", reservation.PMSGuestId);
                    return ApiResponse<bool>.Fail("Guest mapping not found");
                }

                var guestId = guestMapping.GuestFlowGuestId;

                // Invoice oluştur veya güncelle
                // Folio ID'ye göre invoice'u bul (Notes içinde Folio ID saklanıyor)
                var folioDate = pmsFolio.FolioDate?.Date ?? DateTime.UtcNow.Date;
                var existingInvoice = await _invoiceRepository
                    .GetAll(i => i.GuestId == guestId && 
                                i.Notes != null && 
                                i.Notes.Contains($"Folio ID: {pmsFolio.FolioId}"))
                    .OrderByDescending(i => i.CreatedDate)
                    .FirstOrDefaultAsync();

                InvoicesEntity invoice;

                if (existingInvoice == null)
                {
                    // Yeni invoice oluştur
                    var invoiceNumber = await GenerateInvoiceNumberAsync();
                    invoice = new InvoicesEntity
                    {
                        InvoiceNumber = invoiceNumber,
                        IssueDate = pmsFolio.FolioDate ?? DateTime.UtcNow,
                        TotalAmount = pmsFolio.TotalAmount,
                        Currency = pmsFolio.Currency ?? "TRY",
                        GuestId = guestId,
                        Status = pmsFolio.Status.ToUpperInvariant() == "CLOSED" || pmsFolio.Status.ToUpperInvariant() == "SETTLED" 
                            ? InvoiceStatus.Paid 
                            : InvoiceStatus.Draft,
                        Notes = $"Synced from {source} - Folio ID: {pmsFolio.FolioId} | Reservation: {pmsFolio.ReservationId} | Status: {pmsFolio.Status}"
                    };

                    await _invoiceRepository.AddAsync(invoice);
                    await _unitOfWork.CommitAsync(); // Invoice ID'yi almak için commit
                }
                else
                {
                    invoice = existingInvoice;
                    // Mevcut invoice'u güncelle (sadece draft veya unpaid ise)
                    if (invoice.CanBeModified || invoice.Status == InvoiceStatus.Unpaid)
                    {
                        invoice.TotalAmount = pmsFolio.TotalAmount;
                        invoice.Currency = pmsFolio.Currency ?? invoice.Currency;
                        
                        // Status güncelleme
                        if (pmsFolio.Status.ToUpperInvariant() == "CLOSED" || pmsFolio.Status.ToUpperInvariant() == "SETTLED")
                        {
                            invoice.Status = InvoiceStatus.Paid;
                        }
                        else if (pmsFolio.Status.ToUpperInvariant() == "OPEN" && invoice.Status == InvoiceStatus.Draft)
                        {
                            invoice.Status = InvoiceStatus.Unpaid;
                        }

                        invoice.Notes = $"Synced from {source} - Folio ID: {pmsFolio.FolioId} | Reservation: {pmsFolio.ReservationId} | Status: {pmsFolio.Status}";
                        invoice.MarkAsUpdated();
                        _invoiceRepository.Update(invoice);

                        // Mevcut PMS folio items'ları sil (yenilerini ekleyeceğiz)
                        var existingPMSItems = await _invoiceItemRepository
                            .GetAll(ii => ii.InvoiceId == invoice.Id && ii.ServiceType == "PMSFolio")
                            .ToListAsync();

                        foreach (var item in existingPMSItems)
                        {
                            await _invoiceItemRepository.DeleteAsync(item);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Invoice {InvoiceId} cannot be modified (Status: {Status}), skipping folio sync", 
                            invoice.Id, invoice.Status);
                        return ApiResponse<bool>.SuccessResponse(true, "Invoice locked, sync skipped");
                    }
                }

                // Folio items'ları invoice items'a çevir
                if (pmsFolio.Items != null && pmsFolio.Items.Any())
                {
                    int itemIndex = 0;
                    foreach (var folioItem in pmsFolio.Items)
                    {
                        var invoiceItem = new InvoiceItemEntity
                        {
                            InvoiceId = invoice.Id,
                            ServiceType = "PMSFolio", // Özel service type for PMS folio items
                            ServiceId = itemIndex, // Item index as ServiceId
                            Amount = folioItem.Amount,
                            Currency = pmsFolio.Currency ?? "TRY",
                            Notes = $"{folioItem.Description} | Category: {folioItem.Category ?? "N/A"}" +
                                    (folioItem.TransactionDate.HasValue ? $" | Date: {folioItem.TransactionDate.Value:yyyy-MM-dd}" : ""),
                            VatRate = 0m, // PMS'den VAT bilgisi gelmiyorsa 0
                            VatAmount = 0m
                        };

                        await _invoiceItemRepository.AddAsync(invoiceItem);
                        itemIndex++;
                    }
                }
                else
                {
                    // Folio items yoksa, toplam tutarı tek bir item olarak ekle
                    var invoiceItem = new InvoiceItemEntity
                    {
                        InvoiceId = invoice.Id,
                        ServiceType = "PMSFolio",
                        ServiceId = 0,
                        Amount = pmsFolio.TotalAmount,
                        Currency = pmsFolio.Currency ?? "TRY",
                        Notes = $"PMS Folio Total - {pmsFolio.FolioId}",
                        VatRate = 0m,
                        VatAmount = 0m
                    };

                    await _invoiceItemRepository.AddAsync(invoiceItem);
                }

                await _unitOfWork.CommitAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Folio synced successfully");
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "Failed to sync folio: {FolioId}", pmsFolio.FolioId);
               return ApiResponse<bool>.Fail($"Failed to sync folio: {ex.Message}");
            }
        }

        private async Task<int> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastInvoice = await _invoiceRepository
                .GetAll()
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                // InvoiceNumber format'ına göre parse et (örn: 20250001)
                var invoiceNumberStr = lastInvoice.InvoiceNumber.ToString();
                if (invoiceNumberStr.Length >= 4 && invoiceNumberStr.StartsWith(year.ToString()))
                {
                    var numberPart = invoiceNumberStr.Substring(4);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            return int.Parse($"{year}{nextNumber:D4}");
        }

        private async Task<string> GenerateGuestCodeAsync()
        {
            // GuestCode oluştur (örn: GUEST-2025-0001)
            var year = DateTime.UtcNow.Year;
            var lastGuest = await _guestRepository
                .GetAll()
                .OrderByDescending(g => g.GuestCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastGuest != null && lastGuest.GuestCode.StartsWith($"GUEST-{year}-"))
            {
                var parts = lastGuest.GuestCode.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"GUEST-{year}-{nextNumber:D4}";
        }

        #endregion
    }
}
