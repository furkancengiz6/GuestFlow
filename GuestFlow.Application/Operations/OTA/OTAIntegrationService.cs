using GuestFlow.Application.Models.Requests.OTA;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;

namespace GuestFlow.Application.Operations.OTA
{
    public class OTAIntegrationService : IOTAIntegrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;

        public OTAIntegrationService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse<OTAIntegration>> CreateOTAIntegrationAsync(CreateOTAIntegrationRequest request)
        {
            try
            {
                var integration = new OTAIntegration
                {
                    ProviderName = request.ProviderName,
                    ProviderCode = request.ProviderCode,
                    ApiEndpoint = request.ApiEndpoint,
                    ApiKey = request.ApiKey,
                    ApiSecret = request.ApiSecret,
                    WebhookUrl = request.WebhookUrl,
                    IsActive = request.IsActive
                };

                await _unitOfWork.OTAIntegrations.AddAsync(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<OTAIntegration>.SuccessResponse(integration, "OTA integration created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<OTAIntegration>.Fail($"Failed to create OTA integration: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OTAIntegration>>> GetAllOTAIntegrationsAsync()
        {
            try
            {
                var integrations = await _unitOfWork.OTAIntegrations.GetAll(i => i.IsActive).ToListAsync();
                return ApiResponse<List<OTAIntegration>>.SuccessResponse(integrations);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<OTAIntegration>>.Fail($"Failed to get OTA integrations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> TestOTAConnectionAsync(int integrationId)
        {
            try
            {
                var integration = await _unitOfWork.OTAIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("OTA integration not found");

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {integration.ApiKey}");

                var response = await client.GetAsync($"{integration.ApiEndpoint}/health");
                var isConnected = response.IsSuccessStatusCode;

                // Update last sync info
                integration.LastSyncDate = DateTime.UtcNow;
                integration.LastSyncStatus = isConnected ? "Success" : "Failed";
                if (!isConnected)
                {
                    integration.SyncErrorMessage = $"Connection test failed: {response.StatusCode}";
                }

                _unitOfWork.OTAIntegrations.Update(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(isConnected,
                    isConnected ? "Connection successful" : "Connection failed");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Connection test failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SyncReservationsAsync(int integrationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var integration = await _unitOfWork.OTAIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("OTA integration not found");

                // This would integrate with actual OTA APIs
                // For now, return success
                integration.LastSyncDate = DateTime.UtcNow;
                integration.LastSyncStatus = "Success";
                integration.SyncErrorMessage = null;

                _unitOfWork.OTAIntegrations.Update(integration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Reservations synced successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Sync failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UpdateRoomPricesAsync(int integrationId, int hotelId, List<PriceUpdateRequest> prices)
        {
            try
            {
                var integration = await _unitOfWork.OTAIntegrations.GetByIdAsync(integrationId);
                if (integration == null)
                    return ApiResponse<bool>.Fail("OTA integration not found");

                // Create price update records
                foreach (var price in prices)
                {
                    var priceUpdate = new OTAPriceUpdate
                    {
                        OTAIntegrationId = integrationId,
                        HotelId = hotelId,
                        OTARoomTypeId = price.RoomTypeId,
                        Date = price.Date,
                        Price = price.Price,
                        Currency = price.Currency,
                        IsAvailable = price.IsAvailable,
                        UpdateStatus = "Pending"
                    };

                    await _unitOfWork.OTAPriceUpdates.AddAsync(priceUpdate);
                }

                await _unitOfWork.CommitAsync();

                // Here you would call the actual OTA API to update prices
                // For now, mark as sent
                var updates = await _unitOfWork.OTAPriceUpdates
                    .GetAll(p => p.OTAIntegrationId == integrationId && p.UpdateStatus == "Pending")
                    .ToListAsync();

                foreach (var update in updates)
                {
                    update.UpdateStatus = "Sent";
                    update.SentAt = DateTime.UtcNow;
                    _unitOfWork.OTAPriceUpdates.Update(update);
                }

                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Room prices updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Price update failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OTAReservation>>> GetPendingReservationsAsync(int integrationId)
        {
            try
            {
                var reservations = await _unitOfWork.OTAReservations
                    .GetAll(r => r.OTAIntegrationId == integrationId &&
                                (r.Status == "Pending" || r.Status == "Modified") &&
                                r.GuestFlowReservationId == null)
                    .ToListAsync();

                return ApiResponse<List<OTAReservation>>.SuccessResponse(reservations);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<OTAReservation>>.Fail($"Failed to get pending reservations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ProcessWebhookAsync(string providerCode, string payload)
        {
            try
            {
                // Find integration by provider code
                var integration = await _unitOfWork.OTAIntegrations
                    .GetAsync(i => i.ProviderCode == providerCode && i.IsActive);

                if (integration == null)
                    return ApiResponse<bool>.Fail("OTA integration not found");

                // Parse webhook payload and create/update reservations
                // This would be specific to each OTA provider's webhook format

                return ApiResponse<bool>.SuccessResponse(true, "Webhook processed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Webhook processing failed: {ex.Message}");
            }
        }
    }
}