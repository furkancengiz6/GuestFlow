// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.OTA
{
    /// <summary>
    /// OTA Channel Manager servisi - PMS'den OTA'lara availability ve rates senkronizasyonu
    /// </summary>
    public class OTAChannelManagerService : IOTAChannelManagerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly IOTAIntegrationService _otaIntegrationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OTAChannelManagerService> _logger;

        public OTAChannelManagerService(
            IUnitOfWork unitOfWork,
            IPMSIntegrationService pmsIntegrationService,
            IOTAIntegrationService otaIntegrationService,
            IHttpClientFactory httpClientFactory,
            ILogger<OTAChannelManagerService> logger)
        {
            _unitOfWork = unitOfWork;
            _pmsIntegrationService = pmsIntegrationService;
            _otaIntegrationService = otaIntegrationService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> SyncAvailabilityFromPMSToOTAsAsync(int pmsIntegrationId, DateTime? date = null)
        {
            try
            {
                var pmsIntegration = await _unitOfWork.PMSIntegrations.GetByIdAsync(pmsIntegrationId);
                if (pmsIntegration == null || !pmsIntegration.IsActive)
                    return ApiResponse<bool>.Fail("PMS integration not found or inactive");

                // PMS'den room status bilgilerini al
                var roomsResponse = await _pmsIntegrationService.GetRoomsStatusAsync(pmsIntegrationId, date);
                if (!roomsResponse.Success || roomsResponse.Data == null)
                {
                    return ApiResponse<bool>.Fail($"Failed to get rooms from PMS: {roomsResponse.Message}");
                }

                var rooms = roomsResponse.Data;
                var targetDate = date ?? DateTime.UtcNow.Date;

                // Aktif OTA entegrasyonlarını al
                var activeOTAIntegrations = await _unitOfWork.OTAIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                if (activeOTAIntegrations.Count == 0)
                {
                    _logger.LogWarning("No active OTA integrations found for availability sync");
                    return ApiResponse<bool>.SuccessResponse(true, "No active OTA integrations to sync");
                }

                int totalSynced = 0;
                int totalFailed = 0;

                foreach (var otaIntegration in activeOTAIntegrations)
                {
                    try
                    {
                        var result = await SyncAvailabilityToOTAAsync(otaIntegration.Id, pmsIntegrationId, targetDate);
                        if (result.Success)
                        {
                            totalSynced++;
                        }
                        else
                        {
                            totalFailed++;
                            _logger.LogWarning("Failed to sync availability to OTA {OTAProvider}: {Message}",
                                otaIntegration.ProviderName, result.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        totalFailed++;
                        _logger.LogError(ex, "Error syncing availability to OTA {OTAProvider}", otaIntegration.ProviderName);
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true,
                    $"Synced availability to {totalSynced} OTA(s). {totalFailed} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync availability from PMS to OTAs: PMSIntegrationId={PMSIntegrationId}",
                    pmsIntegrationId);
                return ApiResponse<bool>.Fail($"Failed to sync availability: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SyncRatesFromPMSToOTAsAsync(int pmsIntegrationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var pmsIntegration = await _unitOfWork.PMSIntegrations.GetByIdAsync(pmsIntegrationId);
                if (pmsIntegration == null || !pmsIntegration.IsActive)
                    return ApiResponse<bool>.Fail("PMS integration not found or inactive");

                // PMS'den rates bilgilerini al
                // Not: PMS adapter'larında rates endpoint'i yok, bu yüzden room status'ten rates çıkarabiliriz
                // veya PMS'den ayrı bir rates endpoint'i ekleyebiliriz
                // Şimdilik room status'ten rates bilgisini çıkarıyoruz

                var roomsResponse = await _pmsIntegrationService.GetRoomsStatusAsync(pmsIntegrationId, null);
                if (!roomsResponse.Success || roomsResponse.Data == null)
                {
                    return ApiResponse<bool>.Fail($"Failed to get rooms from PMS: {roomsResponse.Message}");
                }

                // Aktif OTA entegrasyonlarını al
                var activeOTAIntegrations = await _unitOfWork.OTAIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                if (activeOTAIntegrations.Count == 0)
                {
                    _logger.LogWarning("No active OTA integrations found for rates sync");
                    return ApiResponse<bool>.SuccessResponse(true, "No active OTA integrations to sync");
                }

                int totalSynced = 0;
                int totalFailed = 0;

                foreach (var otaIntegration in activeOTAIntegrations)
                {
                    try
                    {
                        var result = await SyncRatesToOTAAsync(otaIntegration.Id, pmsIntegrationId, startDate, endDate);
                        if (result.Success)
                        {
                            totalSynced++;
                        }
                        else
                        {
                            totalFailed++;
                            _logger.LogWarning("Failed to sync rates to OTA {OTAProvider}: {Message}",
                                otaIntegration.ProviderName, result.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        totalFailed++;
                        _logger.LogError(ex, "Error syncing rates to OTA {OTAProvider}", otaIntegration.ProviderName);
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true,
                    $"Synced rates to {totalSynced} OTA(s). {totalFailed} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync rates from PMS to OTAs: PMSIntegrationId={PMSIntegrationId}",
                    pmsIntegrationId);
                return ApiResponse<bool>.Fail($"Failed to sync rates: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SyncAvailabilityToOTAAsync(int otaIntegrationId, int pmsIntegrationId, DateTime? date = null)
        {
            try
            {
                var otaIntegration = await _unitOfWork.OTAIntegrations.GetByIdAsync(otaIntegrationId);
                if (otaIntegration == null || !otaIntegration.IsActive)
                    return ApiResponse<bool>.Fail("OTA integration not found or inactive");

                var pmsIntegration = await _unitOfWork.PMSIntegrations.GetByIdAsync(pmsIntegrationId);
                if (pmsIntegration == null || !pmsIntegration.IsActive)
                    return ApiResponse<bool>.Fail("PMS integration not found or inactive");

                // PMS'den room status bilgilerini al
                var roomsResponse = await _pmsIntegrationService.GetRoomsStatusAsync(pmsIntegrationId, date);
                if (!roomsResponse.Success || roomsResponse.Data == null)
                {
                    return ApiResponse<bool>.Fail($"Failed to get rooms from PMS: {roomsResponse.Message}");
                }

                var rooms = roomsResponse.Data;
                var targetDate = date ?? DateTime.UtcNow.Date;

                // OTA adapter oluştur
                var adapter = CreateOTAAdapter(otaIntegration);

                // Her room için availability güncelle
                int synced = 0;
                int failed = 0;

                foreach (var room in rooms)
                {
                    try
                    {
                        // OTA hotel mapping'den room type ID'yi bul
                        // Not: OTAHotelMapping entity'si henüz tam implement edilmemiş olabilir
                        // Şimdilik room number'ı direkt kullanıyoruz
                        var otaRoomTypeId = room.RoomNumber; // TODO: OTAHotelMapping'den al

                        // Room'un o tarihte available olup olmadığını kontrol et
                        var isAvailable = room.Status == "Available" || room.Status == "Vacant";

                        // OTA'ya availability gönder
                        await adapter.UpdateAvailabilityAsync(otaRoomTypeId, targetDate, isAvailable);

                        synced++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogWarning(ex, "Failed to sync availability for room {RoomNumber} to OTA {OTAProvider}",
                            room.RoomNumber, otaIntegration.ProviderName);
                    }
                }

                // OTA integration'ın last sync bilgilerini güncelle
                otaIntegration.LastSyncDate = DateTime.UtcNow;
                otaIntegration.LastSyncStatus = failed == 0 ? "Success" : "PartialSuccess";
                _unitOfWork.OTAIntegrations.Update(otaIntegration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true,
                    $"Synced {synced} rooms. {failed} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync availability to OTA: OTAIntegrationId={OTAIntegrationId}, PMSIntegrationId={PMSIntegrationId}",
                    otaIntegrationId, pmsIntegrationId);
                return ApiResponse<bool>.Fail($"Failed to sync availability: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SyncRatesToOTAAsync(int otaIntegrationId, int pmsIntegrationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var otaIntegration = await _unitOfWork.OTAIntegrations.GetByIdAsync(otaIntegrationId);
                if (otaIntegration == null || !otaIntegration.IsActive)
                    return ApiResponse<bool>.Fail("OTA integration not found or inactive");

                var pmsIntegration = await _unitOfWork.PMSIntegrations.GetByIdAsync(pmsIntegrationId);
                if (pmsIntegration == null || !pmsIntegration.IsActive)
                    return ApiResponse<bool>.Fail("PMS integration not found or inactive");

                // PMS'den room status bilgilerini al (rates bilgisi room status'te olabilir)
                var roomsResponse = await _pmsIntegrationService.GetRoomsStatusAsync(pmsIntegrationId, null);
                if (!roomsResponse.Success || roomsResponse.Data == null)
                {
                    return ApiResponse<bool>.Fail($"Failed to get rooms from PMS: {roomsResponse.Message}");
                }

                var rooms = roomsResponse.Data;

                // OTA adapter oluştur
                var adapter = CreateOTAAdapter(otaIntegration);

                int synced = 0;
                int failed = 0;

                // Her gün için rates gönder
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    foreach (var room in rooms)
                    {
                        try
                        {
                            var otaRoomTypeId = room.RoomNumber; // TODO: OTAHotelMapping'den al
                            
                            // Room'dan rate bilgisini al (PMSRoomStatus'te rate field'ı yoksa default değer kullan)
                            // PMSRoomStatus modelinde Rate ve Currency property'leri yok, bu yüzden default değer kullanıyoruz
                            // TODO: PMS'den rate bilgisini almak için PMS adapter'a rate endpoint'i eklenmeli
                            var rate = 0m; // TODO: PMS'den rate bilgisini al
                            var currency = "TRY"; // TODO: PMS'den currency bilgisini al

                            // OTA'ya rate gönder
                            await adapter.UpdateRatesAsync(otaRoomTypeId, date, rate, currency);

                            synced++;
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            _logger.LogWarning(ex, "Failed to sync rate for room {RoomNumber} on {Date} to OTA {OTAProvider}",
                                room.RoomNumber, date, otaIntegration.ProviderName);
                        }
                    }
                }

                // OTA integration'ın last sync bilgilerini güncelle
                otaIntegration.LastSyncDate = DateTime.UtcNow;
                otaIntegration.LastSyncStatus = failed == 0 ? "Success" : "PartialSuccess";
                _unitOfWork.OTAIntegrations.Update(otaIntegration);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true,
                    $"Synced {synced} rates. {failed} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync rates to OTA: OTAIntegrationId={OTAIntegrationId}, PMSIntegrationId={PMSIntegrationId}",
                    otaIntegrationId, pmsIntegrationId);
                return ApiResponse<bool>.Fail($"Failed to sync rates: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SyncAllActiveIntegrationsAsync()
        {
            try
            {
                // Tüm aktif PMS entegrasyonlarını al
                var activePMSIntegrations = await _unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                if (activePMSIntegrations.Count == 0)
                {
                    _logger.LogWarning("No active PMS integrations found for channel manager sync");
                    return ApiResponse<bool>.SuccessResponse(true, "No active PMS integrations to sync");
                }

                int totalSynced = 0;
                int totalFailed = 0;

                foreach (var pmsIntegration in activePMSIntegrations)
                {
                    try
                    {
                        // Availability sync
                        var availabilityResult = await SyncAvailabilityFromPMSToOTAsAsync(pmsIntegration.Id);
                        if (availabilityResult.Success)
                        {
                            totalSynced++;
                        }
                        else
                        {
                            totalFailed++;
                        }

                        // Rates sync (bugünden 30 gün sonrasına kadar)
                        var ratesResult = await SyncRatesFromPMSToOTAsAsync(
                            pmsIntegration.Id,
                            DateTime.UtcNow.Date,
                            DateTime.UtcNow.Date.AddDays(30));
                        
                        if (ratesResult.Success)
                        {
                            totalSynced++;
                        }
                        else
                        {
                            totalFailed++;
                        }
                    }
                    catch (Exception ex)
                    {
                        totalFailed++;
                        _logger.LogError(ex, "Error syncing PMS integration {PMSProvider} to OTAs",
                            pmsIntegration.ProviderName);
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true,
                    $"Synced {totalSynced} integration(s). {totalFailed} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync all active integrations");
                return ApiResponse<bool>.Fail($"Failed to sync all integrations: {ex.Message}");
            }
        }

        /// <summary>
        /// OTA adapter oluştur (helper method)
        /// </summary>
        private BaseOTAAdapter CreateOTAAdapter(OTAIntegration integration)
        {
            var providerCode = integration.ProviderCode.ToUpperInvariant();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            return providerCode switch
            {
                "BKG" or "BOOKING" or "BOOKINGCOM" => new BookingComAdapter(integration, 
                    _httpClientFactory,
                    loggerFactory.CreateLogger<BookingComAdapter>()),
                "EXP" or "EXPEDIA" => new ExpediaAdapter(integration,
                    _httpClientFactory,
                    loggerFactory.CreateLogger<ExpediaAdapter>()),
                _ => throw new NotSupportedException($"OTA provider '{integration.ProviderCode}' is not supported")
            };
        }
    }
}
