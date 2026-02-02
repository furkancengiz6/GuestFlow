// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Application.Operations.Finance.Pricing;
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

        private readonly IOTAReservationMappingService _mappingService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OTAChannelManagerService> _logger;
        private readonly IOTAAdapterFactory _adapterFactory;
        private readonly IDynamicPricingService _dynamicPricingService;

        public OTAChannelManagerService(
            IUnitOfWork unitOfWork,
            IPMSIntegrationService pmsIntegrationService,
            IOTAReservationMappingService mappingService,
            IHttpClientFactory httpClientFactory,
            ILogger<OTAChannelManagerService> logger,
            IOTAAdapterFactory adapterFactory,
            IDynamicPricingService dynamicPricingService)
        {
            _unitOfWork = unitOfWork;
            _pmsIntegrationService = pmsIntegrationService;
            _mappingService = mappingService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _adapterFactory = adapterFactory;
            _dynamicPricingService = dynamicPricingService;
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

                // PMS'den rates bilgilerini al (Folio veya Room endpointlerinden çıkarım yapılabilir)
                // Şimdilik RoomStatus üzerinden gidiyoruz, gerekirse PMS adaptörüne GetRates eklenebilir.
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

                // PMS room status
                var roomsResponse = await _pmsIntegrationService.GetRoomsStatusAsync(pmsIntegrationId, date);
                if (!roomsResponse.Success || roomsResponse.Data == null)
                {
                    return ApiResponse<bool>.Fail($"Failed to get rooms from PMS: {roomsResponse.Message}");
                }

                var rooms = roomsResponse.Data;
                var targetDate = date ?? DateTime.UtcNow.Date;

                // OTA adapter Factory kullanımı
                var adapter = _adapterFactory.CreateAdapter(otaIntegration);

                // Her room için availability güncelle
                int synced = 0;
                int failed = 0;

                foreach (var room in rooms)
                {
                    try
                    {
                        // OTA hotel mapping'den room type ID'yi bul
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


                // PMS'den room type bilgilerini al (Base Price için)
                var roomTypesResponse = await _pmsIntegrationService.GetRoomTypesAsync(pmsIntegrationId);
                if (!roomTypesResponse.Success || roomTypesResponse.Data == null)
                {
                    return ApiResponse<bool>.Fail($"Failed to get room types from PMS: {roomTypesResponse.Message}");
                }

                var roomTypes = roomTypesResponse.Data;

                // OTA adapter Factory kullanımı
                var adapter = _adapterFactory.CreateAdapter(otaIntegration);

                int synced = 0;
                int failed = 0;

                // Her gün için rates gönder
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    foreach (var roomType in roomTypes)
                    {
                        try
                        {
                            var otaRoomTypeId = roomType.RoomTypeId; // Assuming 1:1 mapping for simple cases or handled by adapter
                            
                            // CALCULATE DYNAMIC RATE
                            // PMS'den gelen BasePrice'ı kullanıyoruz
                            int parsedRoomTypeId;
                            int.TryParse(roomType.RoomTypeId, out parsedRoomTypeId); // Handle string IDs gracefully if possible

                            var dynamicRate = await _dynamicPricingService.CalculateRateAsync(parsedRoomTypeId, date, roomType.BasePrice);
                            
                            var finalPrice = dynamicRate.FinalRate;
                            var currency = roomType.Currency;

                            // If Dynamic Pricing says CLOSED, we should also send StopSell, but here we just sync rates.
                            // Ideally, update availability if IsStopped is true.
                            
                            if (dynamicRate.IsStopSell)
                            {
                                // Optional: Update availability to false if stopped
                                await adapter.UpdateAvailabilityAsync(otaRoomTypeId, date, false);
                            }

                            // OTA'ya rate gönder
                            await adapter.UpdateRatesAsync(otaRoomTypeId, date, finalPrice, currency);

                            synced++;
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            _logger.LogWarning(ex, "Failed to sync rate for room type {RoomTypeId} on {Date} to OTA {OTAProvider}",
                                roomType.RoomTypeId, date, otaIntegration.ProviderName);
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

        public async Task<ApiResponse<bool>> ProcessIncomingReservationAsync(int otaIntegrationId, OTAReservationDto reservationDto)
        {
            try
            {
                // 1. Deduplication / Conflict Check
                var duplicateCheck = await _mappingService.CheckDuplicateAsync(reservationDto);
                if (!duplicateCheck.Success || duplicateCheck.Data == false)
                {
                    _logger.LogInformation("Duplicate OTA reservation detected, skipping processing: {OTAReservationId}", reservationDto.OTAReservationId);
                    return ApiResponse<bool>.SuccessResponse(true, "Duplicate reservation ignored");
                }

                // 2. Persistence (Save/Update OTAReservation)
                var existingReservation = await _unitOfWork.OTAReservations
                    .GetAll(r => r.OTAIntegrationId == otaIntegrationId && r.OTAReservationId == reservationDto.OTAReservationId)
                    .FirstOrDefaultAsync();

                if (existingReservation == null)
                {
                    existingReservation = new OTAReservation
                    {
                        OTAIntegrationId = otaIntegrationId,
                        OTAReservationId = reservationDto.OTAReservationId,
                        OTAHotelId = reservationDto.OTAHotelId,
                        OTARoomTypeId = reservationDto.OTARoomTypeId,
                        CheckInDate = reservationDto.CheckInDate,
                        CheckOutDate = reservationDto.CheckOutDate,
                        GuestCount = reservationDto.GuestCount,
                        TotalPrice = reservationDto.TotalPrice,
                        Currency = reservationDto.Currency,
                        GuestName = reservationDto.GuestName,
                        GuestEmail = reservationDto.GuestEmail,
                        GuestPhone = reservationDto.GuestPhone,
                        Status = reservationDto.Status,
                        OTACreatedDate = DateTime.UtcNow,
                        OTALastModifiedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.OTAReservations.AddAsync(existingReservation);
                }
                else
                {
                    existingReservation.CheckInDate = reservationDto.CheckInDate;
                    existingReservation.CheckOutDate = reservationDto.CheckOutDate;
                    existingReservation.Status = reservationDto.Status;
                    existingReservation.TotalPrice = reservationDto.TotalPrice;
                    existingReservation.GuestName = reservationDto.GuestName;
                    existingReservation.OTALastModifiedDate = DateTime.UtcNow;
                    
                    _unitOfWork.OTAReservations.Update(existingReservation);
                }

                await _unitOfWork.CommitAsync();
                
                _logger.LogInformation("OTAReservation {Id} saved.", existingReservation.Id);

                return ApiResponse<bool>.SuccessResponse(true, "Reservation processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing incoming reservation: {OTAReservationId}", reservationDto.OTAReservationId);
                return ApiResponse<bool>.Fail(ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> PushRateUpdateAsync(int otaIntegrationId, string otaRoomTypeId, DateTime date, decimal amount, string currency = "TRY")
        {
            try
            {
                var otaIntegration = await _unitOfWork.OTAIntegrations.GetByIdAsync(otaIntegrationId);
                if (otaIntegration == null || !otaIntegration.IsActive)
                    return ApiResponse<bool>.Fail("OTA integration not found or inactive");

                var adapter = _adapterFactory.CreateAdapter(otaIntegration);

                await adapter.UpdateRatesAsync(otaRoomTypeId, date, amount, currency);

                _logger.LogInformation("Pushed dynamic rate to OTA {Provider}: Room={Room}, Date={Date}, Amount={Amount}", 
                    otaIntegration.ProviderName, otaRoomTypeId, date, amount);

                return ApiResponse<bool>.SuccessResponse(true, "Rate pushed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push rate update to OTA {IntegrationId}", otaIntegrationId);
                return ApiResponse<bool>.Fail($"Failed to push rate: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> BroadcastStopSellAsync(int hotelId, DateTime startDate, DateTime endDate)
        {
            try
            {
                 // Get all active OTA integrations
                var activeOTAIntegrations = await _unitOfWork.OTAIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                if (activeOTAIntegrations.Count == 0)
                    return ApiResponse<bool>.SuccessResponse(true, "No active channels to stop sell.");

                int totalChannels = activeOTAIntegrations.Count;
                int successChannels = 0;

                // For each channel, push availability = false for all room types
                // Note: Ideally we should know which room types belong to the hotel.
                // Assuming we can get room types from PMS or iterating over a known set.
                // For simplified "Emergency Stop", we might need a list of RoomTypeIds.
                
                // Fetch room types from PMS (Room Statuses) to know what to close
                // Assuming we use the first active PMS integration linked to this hotel
                // TODO: Add HotelId to PMSIntegration entity to support multi-property filtering.
                var pmsIntegration = await _unitOfWork.PMSIntegrations.GetAll(p => p.IsActive).FirstOrDefaultAsync();
                if (pmsIntegration == null)
                {
                     // Fallback check: if no PMS specific to hotel, maybe try first one?
                     // Or return error.
                     _logger.LogWarning("No PMS integration found for hotel {HotelId} to identify rooms for Stop Sell", hotelId);
                     // Proceeding with a warning/fail might be better, or we can iterate ALL mapped rooms in DB if we had them.
                }

                // If we found a PMS integration, get rooms
                List<string> roomTypeIds = new List<string>();
                if (pmsIntegration != null)
                {
                    var rooms = await _pmsIntegrationService.GetRoomsStatusAsync(pmsIntegration.Id, null);
                    if (rooms.Success && rooms.Data != null)
                    {
                        roomTypeIds = rooms.Data.Select(r => r.RoomType).Distinct().ToList(); // Use RoomType instead of RoomNumber ideally
                        if (!roomTypeIds.Any()) roomTypeIds = rooms.Data.Select(r => r.RoomNumber).Distinct().ToList();
                    }
                }
                
                if (!roomTypeIds.Any())
                {
                    // Fallback: If we can't find specific rooms, we can't stop sell via room-specific APIs.
                    // Exceptions: Some OTAs have "Close Hotel" API. 
                    return ApiResponse<bool>.Fail("Could not identify room types to Stop Sell.");
                }

                foreach (var ota in activeOTAIntegrations)
                {
                    try
                    {
                        var adapter = _adapterFactory.CreateAdapter(ota);
                        
                        // Iterate dates
                        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                        {
                            foreach (var roomTypeId in roomTypeIds)
                            {
                                 await adapter.UpdateAvailabilityAsync(roomTypeId, date, false);
                            }
                        }
                        successChannels++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to Broadcast Stop Sell to OTA {Provider}", ota.ProviderName);
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true, $"Stop Sell broadcasted to {successChannels}/{totalChannels} channels.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast Stop Sell for hotel {HotelId}", hotelId);
                return ApiResponse<bool>.Fail($"Failed to broadcast Stop Sell: {ex.Message}");
            }
        }
    }
}
