// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Requests.PMS;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.PMS
{
    /// <summary>
    /// PMS entegrasyon servisi - adapter pattern kullanarak farklı PMS provider'ları destekler
    /// </summary>
    public class PMSIntegrationService : IPMSIntegrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PMSIntegrationService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public PMSIntegrationService(
            IUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            ILogger<PMSIntegrationService> logger,
            ILoggerFactory loggerFactory)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public async Task<ApiResponse<PMSIntegration>> CreatePMSIntegrationAsync(CreatePMSIntegrationRequest request)
        {
            try
            {
                var integration = new PMSIntegration
                {
                    ProviderName = request.ProviderName,
                    ProviderCode = request.ProviderCode,
                    ApiEndpoint = request.ApiEndpoint,
                    ApiKey = request.ApiKey,
                    ApiSecret = request.ApiSecret,
                    WebhookUrl = request.WebhookUrl,
                    WebhookSecret = request.WebhookSecret,
                    IsActive = request.IsActive,
                    SyncMode = Enum.Parse<PMSSyncMode>(request.SyncMode),
                    PollingIntervalMinutes = request.PollingIntervalMinutes
                };

                await _unitOfWork.PMSIntegrations.AddAsync(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<PMSIntegration>.SuccessResponse(integration, "PMS integration created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PMS integration");
                return ApiResponse<PMSIntegration>.Fail($"Failed to create PMS integration: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PMSIntegration>>> GetAllPMSIntegrationsAsync()
        {
            try
            {
                var integrations = await _unitOfWork.PMSIntegrations.GetAll().ToListAsync();
                return ApiResponse<List<PMSIntegration>>.SuccessResponse(integrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get PMS integrations");
                return ApiResponse<List<PMSIntegration>>.Fail($"Failed to get PMS integrations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PMSIntegration>> GetPMSIntegrationByIdAsync(int integrationId)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<PMSIntegration>.Fail("PMS integration not found");

                return ApiResponse<PMSIntegration>.SuccessResponse(integration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get PMS integration: {IntegrationId}", integrationId);
                return ApiResponse<PMSIntegration>.Fail($"Failed to get PMS integration: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PMSIntegration>> UpdatePMSIntegrationAsync(int integrationId, UpdatePMSIntegrationRequest request)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<PMSIntegration>.Fail("PMS integration not found");

                if (!string.IsNullOrEmpty(request.ProviderName))
                    integration.ProviderName = request.ProviderName;
                if (!string.IsNullOrEmpty(request.ApiEndpoint))
                    integration.ApiEndpoint = request.ApiEndpoint;
                if (!string.IsNullOrEmpty(request.ApiKey))
                    integration.ApiKey = request.ApiKey;
                if (request.ApiSecret != null)
                    integration.ApiSecret = request.ApiSecret;
                if (request.WebhookUrl != null)
                    integration.WebhookUrl = request.WebhookUrl;
                if (request.WebhookSecret != null)
                    integration.WebhookSecret = request.WebhookSecret;
                if (request.IsActive.HasValue)
                    integration.IsActive = request.IsActive.Value;
                if (!string.IsNullOrEmpty(request.SyncMode))
                    integration.SyncMode = Enum.Parse<PMSSyncMode>(request.SyncMode);
                if (request.PollingIntervalMinutes.HasValue)
                    integration.PollingIntervalMinutes = request.PollingIntervalMinutes.Value;

                integration.MarkAsUpdated();
                _unitOfWork.PMSIntegrations.Update(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<PMSIntegration>.SuccessResponse(integration, "PMS integration updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update PMS integration: {IntegrationId}", integrationId);
                return ApiResponse<PMSIntegration>.Fail($"Failed to update PMS integration: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeletePMSIntegrationAsync(int integrationId)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("PMS integration not found");

                integration.IsDeleted = true;
                integration.MarkAsUpdated();
                _unitOfWork.PMSIntegrations.Update(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true, "PMS integration deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete PMS integration: {IntegrationId}", integrationId);
                return ApiResponse<bool>.Fail($"Failed to delete PMS integration: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> TestPMSConnectionAsync(int integrationId)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var isConnected = await adapter.TestConnectionAsync();

                // Update connection test info
                integration.LastConnectionTestDate = DateTime.UtcNow;
                integration.LastConnectionTestResult = isConnected;
                integration.LastSyncDate = DateTime.UtcNow;
                integration.LastSyncStatus = isConnected ? "Success" : "Failed";
                if (!isConnected)
                {
                    integration.SyncErrorMessage = "Connection test failed";
                }

                _unitOfWork.PMSIntegrations.Update(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(isConnected,
                    isConnected ? "Connection successful" : "Connection failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test PMS connection: {IntegrationId}", integrationId);
                return ApiResponse<bool>.Fail($"Connection test failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> RefreshPMSAccessTokenAsync(int integrationId)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var refreshed = await adapter.RefreshAccessTokenAsync();

                if (refreshed)
                {
                    _unitOfWork.PMSIntegrations.Update(integration);
                    await _unitOfWork.CommitAsync();
                }

                return ApiResponse<bool>.SuccessResponse(refreshed,
                    refreshed ? "Token refreshed successfully" : "Token refresh failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh PMS access token: {IntegrationId}", integrationId);
                return ApiResponse<bool>.Fail($"Token refresh failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PMSGuestProfile>> GetGuestProfileAsync(int integrationId, string pmsGuestId)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<PMSGuestProfile>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var guestProfile = await adapter.GetGuestProfileAsync(pmsGuestId);

                if (guestProfile == null)
                    return ApiResponse<PMSGuestProfile>.Fail("Guest not found in PMS");

                return ApiResponse<PMSGuestProfile>.SuccessResponse(guestProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest profile: {IntegrationId}, {GuestId}", integrationId, pmsGuestId);
                return ApiResponse<PMSGuestProfile>.Fail($"Failed to get guest profile: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PMSGuestProfile>>> GetGuestsAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<List<PMSGuestProfile>>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var guests = await adapter.GetGuestsAsync(startDate, endDate);

                return ApiResponse<List<PMSGuestProfile>>.SuccessResponse(guests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guests: {IntegrationId}", integrationId);
                return ApiResponse<List<PMSGuestProfile>>.Fail($"Failed to get guests: {ex.Message}");
            }
        }


        public async Task<ApiResponse<PMSReservation>> GetReservationAsync(int integrationId, string pmsReservationId)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<PMSReservation>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var reservation = await adapter.GetReservationAsync(pmsReservationId);

                if (reservation == null)
                    return ApiResponse<PMSReservation>.Fail("Reservation not found in PMS");

                return ApiResponse<PMSReservation>.SuccessResponse(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservation: {IntegrationId}, {ReservationId}", integrationId, pmsReservationId);
                return ApiResponse<PMSReservation>.Fail($"Failed to get reservation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PMSReservation>>> GetReservationsAsync(int integrationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<List<PMSReservation>>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var reservations = await adapter.GetReservationsAsync(startDate, endDate);

                return ApiResponse<List<PMSReservation>>.SuccessResponse(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reservations: {IntegrationId}", integrationId);
                return ApiResponse<List<PMSReservation>>.Fail($"Failed to get reservations: {ex.Message}");
            }
        }


        public async Task<ApiResponse<PMSRoomStatus>> GetRoomStatusAsync(int integrationId, string roomNumber)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<PMSRoomStatus>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var roomStatus = await adapter.GetRoomStatusAsync(roomNumber);

                if (roomStatus == null)
                    return ApiResponse<PMSRoomStatus>.Fail("Room not found in PMS");

                return ApiResponse<PMSRoomStatus>.SuccessResponse(roomStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room status: {IntegrationId}, {RoomNumber}", integrationId, roomNumber);
                return ApiResponse<PMSRoomStatus>.Fail($"Failed to get room status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PMSRoomStatus>>> GetRoomsStatusAsync(int integrationId, DateTime? date = null)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<List<PMSRoomStatus>>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var rooms = await adapter.GetRoomsStatusAsync(date);

                return ApiResponse<List<PMSRoomStatus>>.SuccessResponse(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms status: {IntegrationId}", integrationId);
                return ApiResponse<List<PMSRoomStatus>>.Fail($"Failed to get rooms status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PMSRoomType>>> GetRoomTypesAsync(int integrationId)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<List<PMSRoomType>>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var roomTypes = await adapter.GetRoomTypesAsync();

                return ApiResponse<List<PMSRoomType>>.SuccessResponse(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get room types: {IntegrationId}", integrationId);
                return ApiResponse<List<PMSRoomType>>.Fail($"Failed to get room types: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PMSFolio>> GetFolioAsync(int integrationId, string reservationId)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<PMSFolio>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var folio = await adapter.GetFolioAsync(reservationId);

                if (folio == null)
                    return ApiResponse<PMSFolio>.Fail("Folio not found in PMS");

                return ApiResponse<PMSFolio>.SuccessResponse(folio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get folio: {IntegrationId}, {ReservationId}", integrationId, reservationId);
                return ApiResponse<PMSFolio>.Fail($"Failed to get folio: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PMSFolio>>> GetFoliosAsync(int integrationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<List<PMSFolio>>.Fail("PMS integration not found");

                var adapter = CreateAdapter(integration);
                var folios = await adapter.GetFoliosAsync(startDate, endDate);

                return ApiResponse<List<PMSFolio>>.SuccessResponse(folios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get folios: {IntegrationId}", integrationId);
                return ApiResponse<List<PMSFolio>>.Fail($"Failed to get folios: {ex.Message}");
            }
        }



        public async Task<ApiResponse<List<PMSSyncHistoryResponse>>> GetSyncHistoryAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _unitOfWork.PMSSyncHistories.GetAll(s => s.PMSIntegrationId == integrationId);

                if (startDate.HasValue)
                    query = query.Where(s => s.SyncStartTime >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(s => s.SyncStartTime <= endDate.Value);

                var histories = await query
                    .OrderByDescending(s => s.SyncStartTime)
                    .Take(100)
                    .ToListAsync();

                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(integrationId);
                var response = histories.Select(h => new PMSSyncHistoryResponse
                {
                    Id = h.Id,
                    PMSIntegrationId = h.PMSIntegrationId,
                    ProviderName = integration?.ProviderName ?? "Unknown",
                    SyncType = h.SyncType.ToString(),
                    EntityType = h.EntityType,
                    EntityId = h.EntityId,
                    Status = h.Status.ToString(),
                    SyncStartTime = h.SyncStartTime,
                    SyncEndTime = h.SyncEndTime,
                    RecordsProcessed = h.RecordsProcessed,
                    RecordsSucceeded = h.RecordsSucceeded,
                    RecordsFailed = h.RecordsFailed,
                    ErrorMessage = h.ErrorMessage
                }).ToList();

                return ApiResponse<List<PMSSyncHistoryResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get sync history: {IntegrationId}", integrationId);
                return ApiResponse<List<PMSSyncHistoryResponse>>.Fail($"Failed to get sync history: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PMSSyncHistoryResponse>> GetSyncHistoryByIdAsync(int syncHistoryId)
        {
            try
            {
                var history = await _unitOfWork.PMSSyncHistories.GetByIdAsync(syncHistoryId);
                if (history == null)
                    return ApiResponse<PMSSyncHistoryResponse>.Fail("Sync history not found");

                var integration = await _unitOfWork.PMSIntegrations.GetByIdAsync(history.PMSIntegrationId);
                var response = new PMSSyncHistoryResponse
                {
                    Id = history.Id,
                    PMSIntegrationId = history.PMSIntegrationId,
                    ProviderName = integration?.ProviderName ?? "Unknown",
                    SyncType = history.SyncType.ToString(),
                    EntityType = history.EntityType,
                    EntityId = history.EntityId,
                    Status = history.Status.ToString(),
                    SyncStartTime = history.SyncStartTime,
                    SyncEndTime = history.SyncEndTime,
                    RecordsProcessed = history.RecordsProcessed,
                    RecordsSucceeded = history.RecordsSucceeded,
                    RecordsFailed = history.RecordsFailed,
                    ErrorMessage = history.ErrorMessage
                };

                return ApiResponse<PMSSyncHistoryResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get sync history: {SyncHistoryId}", syncHistoryId);
                return ApiResponse<PMSSyncHistoryResponse>.Fail($"Failed to get sync history: {ex.Message}");
            }
        }


        /// <summary>
        /// Provider'a göre uygun adapter'ı oluşturur
        /// </summary>
        private BasePMSAdapter CreateAdapter(PMSIntegration integration)
        {
            var providerCode = integration.ProviderCode.ToUpperInvariant();
            
            return providerCode switch
            {
                "MOCK" => new MockPMSAdapter(integration, _httpClientFactory,
                    _loggerFactory.CreateLogger<MockPMSAdapter>()),
                "OPERA" => new OperaPMSAdapter(integration, _httpClientFactory, 
                    _loggerFactory.CreateLogger<OperaPMSAdapter>()),
                "ELEKTRAWEB" => new ElektrawebPMSAdapter(integration, _httpClientFactory, 
                    _loggerFactory.CreateLogger<ElektrawebPMSAdapter>()),
                _ => throw new NotSupportedException($"PMS provider '{integration.ProviderCode}' is not supported")
            };
        }
    }
}
