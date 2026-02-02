// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;

namespace GuestFlow.Application.Operations.OTA
{
    /// <summary>
    /// OTA Channel Manager servisi - PMS'den OTA'lara availability ve rates senkronizasyonu
    /// </summary>
    public interface IOTAChannelManagerService
    {
        /// <summary>
        /// PMS'den room status bilgilerini al ve tüm aktif OTA'lara availability gönder
        /// </summary>
        Task<ApiResponse<bool>> SyncAvailabilityFromPMSToOTAsAsync(int pmsIntegrationId, DateTime? date = null);

        /// <summary>
        /// PMS'den rates bilgilerini al ve tüm aktif OTA'lara gönder
        /// </summary>
        Task<ApiResponse<bool>> SyncRatesFromPMSToOTAsAsync(int pmsIntegrationId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Belirli bir OTA'ya availability gönder
        /// </summary>
        Task<ApiResponse<bool>> SyncAvailabilityToOTAAsync(int otaIntegrationId, int pmsIntegrationId, DateTime? date = null);

        /// <summary>
        /// Belirli bir OTA'ya rates gönder
        /// </summary>
        Task<ApiResponse<bool>> SyncRatesToOTAAsync(int otaIntegrationId, int pmsIntegrationId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Tüm aktif PMS ve OTA entegrasyonları için otomatik senkronizasyon
        /// </summary>
        Task<ApiResponse<bool>> SyncAllActiveIntegrationsAsync();

        /// <summary>
        /// OTA'dan gelen rezervasyonu işle (Orchestration)
        /// </summary>
        Task<ApiResponse<bool>> ProcessIncomingReservationAsync(int otaIntegrationId, OTAReservationDto reservationDto);

        /// <summary>
        /// Explicitly push a rate update to an OTA (used by Dynamic Pricing)
        /// </summary>
        Task<ApiResponse<bool>> PushRateUpdateAsync(int otaIntegrationId, string otaRoomTypeId, DateTime date, decimal amount, string currency = "TRY");

        /// <summary>
        /// Broadcast Stop Sell to all active OTAs (Emergency Stop)
        /// </summary>
        Task<ApiResponse<bool>> BroadcastStopSellAsync(int hotelId, DateTime startDate, DateTime endDate);
    }
}
