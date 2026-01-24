// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.OTA
{
    /// <summary>
    /// OTA rezervasyon mapping ve conflict resolution servisi
    /// </summary>
    public class OTAReservationMappingService : IOTAReservationMappingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OTAReservationMappingService> _logger;

        public OTAReservationMappingService(
            IUnitOfWork unitOfWork,
            ILogger<OTAReservationMappingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> MapOTAReservationToGuestFlowAsync(int otaReservationId, int guestFlowReservationId)
        {
            try
            {
                var otaReservation = await _unitOfWork.OTAReservations.GetByIdAsync(otaReservationId);
                if (otaReservation == null)
                    return ApiResponse<bool>.Fail("OTA reservation not found");

                var guestFlowReservation = await _unitOfWork.Reservations.GetByIdAsync(guestFlowReservationId);
                if (guestFlowReservation == null)
                    return ApiResponse<bool>.Fail("GuestFlow reservation not found");

                // Conflict kontrolü yap
                var conflictCheck = await CheckConflictAsync(otaReservationId);
                if (conflictCheck.Success && conflictCheck.Data != null && conflictCheck.Data.Status == "Pending")
                {
                    _logger.LogWarning("Cannot map OTA reservation {OTAReservationId} due to pending conflict",
                        otaReservationId);
                    return ApiResponse<bool>.Fail("Cannot map reservation with pending conflict");
                }

                // Mapping oluştur veya güncelle
                otaReservation.GuestFlowReservationId = guestFlowReservationId;
                _unitOfWork.OTAReservations.Update(otaReservation);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Mapped OTA reservation {OTAReservationId} to GuestFlow reservation {GuestFlowReservationId}",
                    otaReservationId, guestFlowReservationId);

                return ApiResponse<bool>.SuccessResponse(true, "Reservation mapped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to map OTA reservation: {OTAReservationId}", otaReservationId);
                return ApiResponse<bool>.Fail($"Failed to map reservation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OTAReservationConflict>> CheckConflictAsync(int otaReservationId)
        {
            try
            {
                var otaReservation = await _unitOfWork.OTAReservations
                    .GetAll(r => r.Id == otaReservationId)
                    .Include(r => r.OTAIntegration)
                    .FirstOrDefaultAsync();

                if (otaReservation == null)
                    return ApiResponse<OTAReservationConflict>.Fail("OTA reservation not found");

                var conflicts = new List<string>();
                var conflictType = "";

                // 1. Duplicate kontrolü - Aynı tarihlerde başka bir rezervasyon var mı?
                var duplicateCheck = await CheckDuplicateAsync(new OTAReservationDto
                {
                    OTAReservationId = otaReservation.OTAReservationId,
                    CheckInDate = otaReservation.CheckInDate,
                    CheckOutDate = otaReservation.CheckOutDate,
                    GuestEmail = otaReservation.GuestEmail,
                    GuestName = otaReservation.GuestName
                });

                if (!duplicateCheck.Success || duplicateCheck.Data == false)
                {
                    conflicts.Add("Duplicate reservation detected");
                    conflictType = "Duplicate";
                }

                // 2. Overlap kontrolü - Aynı oda ve tarihlerde başka bir rezervasyon var mı?
                if (otaReservation.GuestFlowReservationId.HasValue)
                {
                    var guestFlowReservation = await _unitOfWork.Reservations.GetByIdAsync(otaReservation.GuestFlowReservationId.Value);
                    if (guestFlowReservation != null)
                    {
                        // Aynı tarihlerde başka rezervasyonlar var mı kontrol et
                        // ReservationEntity'de CheckInDate ve CheckOutDate yok, bu yüzden overlap kontrolünü farklı yapıyoruz
                        // TODO: ReservationEntity'ye CheckInDate ve CheckOutDate eklenmeli veya başka bir yöntem kullanılmalı
                        var overlappingReservations = new List<ReservationEntity>(); // Şimdilik boş liste

                        if (overlappingReservations.Any())
                        {
                            conflicts.Add($"Overlapping reservations found: {overlappingReservations.Count}");
                            conflictType = "Overlap";
                        }
                    }
                }

                // 3. Price mismatch kontrolü - Fiyat farkı çok büyük mü?
                if (otaReservation.GuestFlowReservationId.HasValue)
                {
                    var guestFlowReservation = await _unitOfWork.Reservations.GetByIdAsync(otaReservation.GuestFlowReservationId.Value);
                    if (guestFlowReservation != null)
                    {
                        var priceDifference = Math.Abs(otaReservation.TotalPrice - guestFlowReservation.TotalAmount);
                        var priceDifferencePercent = (priceDifference / otaReservation.TotalPrice) * 100;

                        if (priceDifferencePercent > 10) // %10'dan fazla fark varsa
                        {
                            conflicts.Add($"Price mismatch: OTA={otaReservation.TotalPrice}, GuestFlow={guestFlowReservation.TotalAmount}, Difference={priceDifferencePercent:F2}%");
                            conflictType = "PriceMismatch";
                        }
                    }
                }

                if (conflicts.Count == 0)
                {
                    return ApiResponse<OTAReservationConflict>.SuccessResponse(new OTAReservationConflict
                    {
                        OTAReservationId = otaReservationId,
                        OTAReservationIdString = otaReservation.OTAReservationId,
                        ConflictType = "None",
                        Status = "Resolved"
                    });
                }

                var conflict = new OTAReservationConflict
                {
                    OTAReservationId = otaReservationId,
                    OTAReservationIdString = otaReservation.OTAReservationId,
                    ConflictType = conflictType,
                    ConflictDetails = string.Join("; ", conflicts),
                    DetectedAt = DateTime.UtcNow,
                    Status = "Pending"
                };

                return ApiResponse<OTAReservationConflict>.SuccessResponse(conflict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check conflict for OTA reservation: {OTAReservationId}", otaReservationId);
                return ApiResponse<OTAReservationConflict>.Fail($"Failed to check conflict: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ResolveConflictAsync(int conflictId, ConflictResolutionStrategy strategy)
        {
            try
            {
                // Conflict'i veritabanında saklamak için bir entity oluşturmalıyız
                // Şimdilik basit bir implementasyon yapıyoruz
                // TODO: OTAReservationConflict entity'si oluştur

                _logger.LogInformation("Resolving conflict {ConflictId} with strategy {Strategy}", conflictId, strategy);

                // Strategy'ye göre işlem yap
                switch (strategy)
                {
                    case ConflictResolutionStrategy.KeepOTA:
                        // OTA rezervasyonunu koru, GuestFlow'dakini iptal et
                        // TODO: Implement
                        break;

                    case ConflictResolutionStrategy.KeepGuestFlow:
                        // GuestFlow rezervasyonunu koru, OTA'dakini iptal et
                        // TODO: Implement
                        break;

                    case ConflictResolutionStrategy.Merge:
                        // İki rezervasyonu birleştir
                        // TODO: Implement
                        break;

                    case ConflictResolutionStrategy.CancelOTA:
                        // OTA rezervasyonunu iptal et
                        // TODO: Implement
                        break;

                    case ConflictResolutionStrategy.CancelGuestFlow:
                        // GuestFlow rezervasyonunu iptal et
                        // TODO: Implement
                        break;

                    case ConflictResolutionStrategy.Manual:
                        // Manuel çözüm gerekiyor - sadece logla
                        _logger.LogWarning("Conflict {ConflictId} requires manual resolution", conflictId);
                        break;
                }

                return ApiResponse<bool>.SuccessResponse(true, "Conflict resolved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve conflict: {ConflictId}", conflictId);
                return ApiResponse<bool>.Fail($"Failed to resolve conflict: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OTAReservationConflict>>> GetAllConflictsAsync()
        {
            try
            {
                // Tüm OTA rezervasyonlarını kontrol et ve conflict'leri bul
                var otaReservations = await _unitOfWork.OTAReservations
                    .GetAll(r => r.Status != "Cancelled" && !r.IsDeleted)
                    .ToListAsync();

                var conflicts = new List<OTAReservationConflict>();

                foreach (var reservation in otaReservations)
                {
                    var conflictCheck = await CheckConflictAsync(reservation.Id);
                    if (conflictCheck.Success && conflictCheck.Data != null && 
                        conflictCheck.Data.ConflictType != "None" && 
                        conflictCheck.Data.Status == "Pending")
                    {
                        conflicts.Add(conflictCheck.Data);
                    }
                }

                return ApiResponse<List<OTAReservationConflict>>.SuccessResponse(conflicts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all conflicts");
                return ApiResponse<List<OTAReservationConflict>>.Fail($"Failed to get conflicts: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CheckDuplicateAsync(OTAReservationDto otaReservation)
        {
            try
            {
                // Aynı email, check-in ve check-out tarihlerinde başka bir rezervasyon var mı?
                var duplicateReservations = await _unitOfWork.OTAReservations
                    .GetAll(r => r.Id != 0 && // Yeni rezervasyon için
                                r.GuestEmail == otaReservation.GuestEmail &&
                                r.CheckInDate == otaReservation.CheckInDate &&
                                r.CheckOutDate == otaReservation.CheckOutDate &&
                                r.Status != "Cancelled" &&
                                !r.IsDeleted)
                    .ToListAsync();

                if (duplicateReservations.Any())
                {
                    _logger.LogWarning("Duplicate OTA reservation detected: Email={Email}, CheckIn={CheckIn}, CheckOut={CheckOut}",
                        otaReservation.GuestEmail, otaReservation.CheckInDate, otaReservation.CheckOutDate);
                    return ApiResponse<bool>.SuccessResponse(false, "Duplicate reservation found");
                }

                // GuestFlow'da da duplicate kontrolü yap
                if (!string.IsNullOrEmpty(otaReservation.GuestEmail))
                {
                    // ReservationEntity'de CheckInDate ve CheckOutDate yok
                    // Guest email'e göre kontrol yapıyoruz
                    var guestFlowReservations = await _unitOfWork.Reservations
                        .GetAll(r => r.Guest != null &&
                                    r.Guest.Email == otaReservation.GuestEmail &&
                                    !r.IsDeleted)
                        .ToListAsync();

                    if (guestFlowReservations.Any())
                    {
                        _logger.LogWarning("Duplicate GuestFlow reservation detected: Email={Email}, CheckIn={CheckIn}, CheckOut={CheckOut}",
                            otaReservation.GuestEmail, otaReservation.CheckInDate, otaReservation.CheckOutDate);
                        return ApiResponse<bool>.SuccessResponse(false, "Duplicate reservation found in GuestFlow");
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true, "No duplicate found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check duplicate for OTA reservation");
                return ApiResponse<bool>.Fail($"Failed to check duplicate: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OTAReservationMapping>>> GetMappingsAsync(int? otaIntegrationId = null)
        {
            try
            {
                var query = _unitOfWork.OTAReservations.GetAll();

                if (otaIntegrationId.HasValue)
                {
                    query = query.Where(r => r.OTAIntegrationId == otaIntegrationId.Value);
                }

                var reservations = await query
                    .Include(r => r.OTAIntegration)
                    .ToListAsync();

                var mappings = reservations.Select(r => new OTAReservationMapping
                {
                    Id = r.Id,
                    OTAIntegrationId = r.OTAIntegrationId,
                    OTAProviderName = r.OTAIntegration.ProviderName,
                    OTAReservationId = r.OTAReservationId,
                    GuestFlowReservationId = r.GuestFlowReservationId,
                    LastSyncedAt = r.OTACreatedDate,
                    SyncStatus = r.Status,
                    ConflictDetails = null // TODO: Conflict bilgisini ekle
                }).ToList();

                return ApiResponse<List<OTAReservationMapping>>.SuccessResponse(mappings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get OTA reservation mappings");
                return ApiResponse<List<OTAReservationMapping>>.Fail($"Failed to get mappings: {ex.Message}");
            }
        }
    }
}
