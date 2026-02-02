// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Requests.PMS;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Domain.Entities.Operations;

namespace GuestFlow.Application.Operations.PMS
{
    /// <summary>
    /// PMS (Property Management System) entegrasyon servisi interface'i
    /// Generic adapter pattern - farklı PMS provider'lar için
    /// </summary>
    public interface IPMSIntegrationService
    {
        // PMS Integration CRUD
        Task<ApiResponse<PMSIntegration>> CreatePMSIntegrationAsync(CreatePMSIntegrationRequest request);
        Task<ApiResponse<List<PMSIntegration>>> GetAllPMSIntegrationsAsync();
        Task<ApiResponse<PMSIntegration>> GetPMSIntegrationByIdAsync(int integrationId);
        Task<ApiResponse<PMSIntegration>> UpdatePMSIntegrationAsync(int integrationId, UpdatePMSIntegrationRequest request);
        Task<ApiResponse<bool>> DeletePMSIntegrationAsync(int integrationId);

        // Connection & Testing
        Task<ApiResponse<bool>> TestPMSConnectionAsync(int integrationId);
        Task<ApiResponse<bool>> RefreshPMSAccessTokenAsync(int integrationId);

        // Guest Operations
        Task<ApiResponse<PMSGuestProfile>> GetGuestProfileAsync(int integrationId, string pmsGuestId);
        Task<ApiResponse<List<PMSGuestProfile>>> GetGuestsAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null);

        // Reservation Operations
        Task<ApiResponse<PMSReservation>> GetReservationAsync(int integrationId, string pmsReservationId);
        Task<ApiResponse<List<PMSReservation>>> GetReservationsAsync(int integrationId, DateTime startDate, DateTime endDate);

        // Room Operations
        Task<ApiResponse<PMSRoomStatus>> GetRoomStatusAsync(int integrationId, string roomNumber);
        Task<ApiResponse<List<PMSRoomStatus>>> GetRoomsStatusAsync(int integrationId, DateTime? date = null);
        
        // Room Type Operations
        Task<ApiResponse<List<PMSRoomType>>> GetRoomTypesAsync(int integrationId);

        // Folio Operations
        Task<ApiResponse<PMSFolio>> GetFolioAsync(int integrationId, string reservationId);
        Task<ApiResponse<List<PMSFolio>>> GetFoliosAsync(int integrationId, DateTime startDate, DateTime endDate);

        // Sync History
        Task<ApiResponse<List<PMSSyncHistoryResponse>>> GetSyncHistoryAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<PMSSyncHistoryResponse>> GetSyncHistoryByIdAsync(int syncHistoryId);
    }
}
