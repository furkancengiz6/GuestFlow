using GuestFlow.Application.Models.Requests.OTA;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Domain.Entities.Operations;

namespace GuestFlow.Application.Operations.OTA
{
    public interface IOTAIntegrationService
    {
        Task<ApiResponse<OTAIntegration>> CreateOTAIntegrationAsync(CreateOTAIntegrationRequest request);
        Task<ApiResponse<List<OTAIntegration>>> GetAllOTAIntegrationsAsync();
        Task<ApiResponse<bool>> TestOTAConnectionAsync(int integrationId);
        Task<ApiResponse<bool>> SyncReservationsAsync(int integrationId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<bool>> UpdateRoomPricesAsync(int integrationId, int hotelId, List<PriceUpdateRequest> prices);
        Task<ApiResponse<List<OTAReservation>>> GetPendingReservationsAsync(int integrationId);
        Task<ApiResponse<bool>> ProcessWebhookAsync(
            string providerCode, 
            string payload, 
            string? signature = null,
            string? idempotencyKey = null,
            string? ipAddress = null,
            string? userAgent = null);
        Task<ApiResponse<object>> GetDeadLetterWebhooksAsync(string? providerCode = null, int pageNumber = 1, int pageSize = 20);
        Task<ApiResponse<bool>> RetryDeadLetterWebhookAsync(int webhookLogId);
    }
}