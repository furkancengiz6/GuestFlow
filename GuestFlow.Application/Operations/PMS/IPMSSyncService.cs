// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Domain.Entities.Operations;

namespace GuestFlow.Application.Operations.PMS
{
    /// <summary>
    /// PMS senkronizasyon servisi interface'i
    /// Real-time, polling ve batch senkronizasyon işlemlerini yönetir
    /// </summary>
    public interface IPMSSyncService
    {
        /// <summary>
        /// Misafirleri senkronize et
        /// </summary>
        Task<ApiResponse<bool>> SyncGuestsAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Rezervasyonları senkronize et
        /// </summary>
        Task<ApiResponse<bool>> SyncReservationsAsync(int integrationId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Oda durumlarını senkronize et
        /// </summary>
        Task<ApiResponse<bool>> SyncRoomsStatusAsync(int integrationId, DateTime? date = null);

        /// <summary>
        /// Folio'ları senkronize et
        /// </summary>
        Task<ApiResponse<bool>> SyncFoliosAsync(int integrationId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Tam senkronizasyon (tüm entity'ler)
        /// </summary>
        Task<ApiResponse<bool>> PerformFullSyncAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Tekil misafir senkronize et (Upsert)
        /// </summary>
        Task<ApiResponse<bool>> SyncGuestAsync(int integrationId, PMSGuestProfile pmsGuest);

        /// <summary>
        /// Tekil rezervasyon senkronize et (Upsert)
        /// </summary>
        Task<ApiResponse<bool>> SyncReservationAsync(int integrationId, PMSReservation pmsReservation);

        /// <summary>
        /// Tekil oda durumu senkronize et
        /// </summary>
        Task<ApiResponse<bool>> SyncRoomStatusAsync(int integrationId, PMSRoomStatus pmsRoomStatus);

        /// <summary>
        /// Tekil folio senkronize et
        /// </summary>
        Task<ApiResponse<bool>> SyncFolioAsync(int integrationId, PMSFolio pmsFolio);

        /// <summary>
        /// Webhook'dan gelen veriyi işle ve senkronize et
        /// </summary>
        Task<ApiResponse<bool>> ProcessWebhookAsync(int integrationId, string payload, string? signature = null);

        /// <summary>
        /// Aktif entegrasyonlar için polling senkronizasyonu başlat
        /// </summary>
        Task ProcessPollingSyncAsync(CancellationToken cancellationToken = default);
    }
}
