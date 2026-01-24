// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Domain.Entities.Operations;

namespace GuestFlow.Application.Operations.OTA
{
    /// <summary>
    /// OTA rezervasyon mapping ve conflict resolution servisi
    /// </summary>
    public interface IOTAReservationMappingService
    {
        /// <summary>
        /// OTA rezervasyonunu GuestFlow rezervasyonu ile eşleştir
        /// </summary>
        Task<ApiResponse<bool>> MapOTAReservationToGuestFlowAsync(int otaReservationId, int guestFlowReservationId);

        /// <summary>
        /// OTA rezervasyonu için conflict kontrolü yap
        /// </summary>
        Task<ApiResponse<OTAReservationConflict>> CheckConflictAsync(int otaReservationId);

        /// <summary>
        /// Conflict'i çöz (manual veya automatic)
        /// </summary>
        Task<ApiResponse<bool>> ResolveConflictAsync(int conflictId, ConflictResolutionStrategy strategy);

        /// <summary>
        /// Tüm conflict'leri listele
        /// </summary>
        Task<ApiResponse<List<OTAReservationConflict>>> GetAllConflictsAsync();

        /// <summary>
        /// OTA rezervasyonu için duplicate kontrolü yap
        /// </summary>
        Task<ApiResponse<bool>> CheckDuplicateAsync(OTAReservationDto otaReservation);

        /// <summary>
        /// OTA rezervasyon mapping'lerini getir
        /// </summary>
        Task<ApiResponse<List<OTAReservationMapping>>> GetMappingsAsync(int? otaIntegrationId = null);
    }

    /// <summary>
    /// OTA rezervasyon conflict modeli
    /// </summary>
    public class OTAReservationConflict
    {
        public int Id { get; set; }
        public int OTAReservationId { get; set; }
        public string OTAReservationIdString { get; set; } = string.Empty;
        public string ConflictType { get; set; } = string.Empty; // Duplicate, Overlap, PriceMismatch
        public string ConflictDetails { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Resolved, Ignored
        public int? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionStrategy { get; set; }
    }

    /// <summary>
    /// Conflict resolution stratejisi
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        KeepOTA,           // OTA rezervasyonunu koru
        KeepGuestFlow,     // GuestFlow rezervasyonunu koru
        Merge,             // İki rezervasyonu birleştir
        CancelOTA,         // OTA rezervasyonunu iptal et
        CancelGuestFlow,   // GuestFlow rezervasyonunu iptal et
        Manual             // Manuel çözüm gerekiyor
    }

    /// <summary>
    /// OTA rezervasyon mapping modeli
    /// </summary>
    public class OTAReservationMapping
    {
        public int Id { get; set; }
        public int OTAIntegrationId { get; set; }
        public string OTAProviderName { get; set; } = string.Empty;
        public string OTAReservationId { get; set; } = string.Empty;
        public int? GuestFlowReservationId { get; set; }
        public DateTime LastSyncedAt { get; set; }
        public string SyncStatus { get; set; } = string.Empty;
        public string? ConflictDetails { get; set; }
    }
}
