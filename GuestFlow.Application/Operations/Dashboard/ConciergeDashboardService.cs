// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.PMS;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Dashboard.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Dashboard
{
    /// <summary>
    /// Concierge Dashboard servisi - PMS entegrasyonlu concierge operasyonları için
    /// </summary>
    public class ConciergeDashboardService : IConciergeDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<RoomAssignmentEntity> _roomAssignmentRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<RestaurantReservationEntity> _restaurantReservationRepository;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly IGuestPreferenceAnalysisService _preferenceAnalysisService;
        private readonly DailyOperationsService _dailyOperationsService;
        private readonly ILogger<ConciergeDashboardService> _logger;

        public ConciergeDashboardService(
            IUnitOfWork unitOfWork,
            IRepository<GuestEntity> guestRepository,
            IRepository<RoomAssignmentEntity> roomAssignmentRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<RestaurantReservationEntity> restaurantReservationRepository,
            IPMSIntegrationService pmsIntegrationService,
            IGuestPreferenceAnalysisService preferenceAnalysisService,
            DailyOperationsService dailyOperationsService,
            ILogger<ConciergeDashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _guestRepository = guestRepository;
            _roomAssignmentRepository = roomAssignmentRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _restaurantReservationRepository = restaurantReservationRepository;
            _pmsIntegrationService = pmsIntegrationService;
            _preferenceAnalysisService = preferenceAnalysisService;
            _dailyOperationsService = dailyOperationsService;
            _logger = logger;
        }

        public async Task<ConciergeCheckInOutDto> GetTodayCheckInsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var result = new ConciergeCheckInOutDto
                {
                    Date = today,
                    Items = new List<CheckInOutItemDto>()
                };

                // GuestFlow'dan bugünkü check-in'leri getir
                var guestFlowCheckIns = await _guestRepository.GetAll()
                    .Where(g => g.CheckInDate.HasValue && 
                               g.CheckInDate.Value.Date == today && 
                               !g.IsDeleted)
                    .Select(g => new CheckInOutItemDto
                    {
                        GuestId = g.Id,
                        GuestName = g.FullName,
                        GuestCode = g.GuestCode,
                        RoomNumber = g.RoomNumber,
                        CheckInDate = g.CheckInDate,
                        CheckOutDate = g.CheckOutDate,
                        Email = g.Email,
                        PhoneNumber = g.PhoneNumber,
                        IsVIP = g.IsSpecialGuest,
                        Source = "GuestFlow"
                    })
                    .ToListAsync();

                result.Items.AddRange(guestFlowCheckIns);

                // PMS'den bugünkü check-in'leri getir
                var activePMSIntegrations = await _unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                foreach (var integration in activePMSIntegrations)
                {
                    try
                    {
                        // Bugünkü rezervasyonları getir
                        var reservationsResponse = await _pmsIntegrationService.GetReservationsAsync(
                            integration.Id, today, today.AddDays(1));

                        if (reservationsResponse.Success && reservationsResponse.Data != null)
                        {
                            foreach (var pmsReservation in reservationsResponse.Data)
                            {
                                // Check-in tarihi bugün mü?
                                if (pmsReservation.CheckInDate.Date == today)
                                {
                                    // Guest mapping'ini bul
                                    var guestMapping = await _unitOfWork.PMSGuestMappings
                                        .GetAll(m => m.PMSIntegrationId == integration.Id && 
                                                    m.PMSGuestId == pmsReservation.PMSGuestId)
                                        .Include(m => m.GuestFlowGuest)
                                        .FirstOrDefaultAsync();

                                    var item = new CheckInOutItemDto
                                    {
                                        GuestId = guestMapping?.GuestFlowGuestId ?? 0,
                                        GuestName = pmsReservation.GuestName ?? string.Empty,
                                        GuestCode = guestMapping?.GuestFlowGuest?.GuestCode ?? string.Empty,
                                        RoomNumber = pmsReservation.RoomNumber,
                                        RoomType = pmsReservation.RoomType,
                                        CheckInDate = pmsReservation.CheckInDate,
                                        CheckOutDate = pmsReservation.CheckOutDate,
                                        NumberOfGuests = null, // PMSReservation'da NumberOfGuests yok
                                        Email = pmsReservation.GuestEmail,
                                        PhoneNumber = pmsReservation.GuestPhone,
                                        Source = "PMS",
                                        PMSReservationId = pmsReservation.PMSReservationId,
                                        PMSProviderName = integration.ProviderName
                                    };

                                    // GuestFlow'da zaten varsa ekleme (duplicate önleme)
                                    if (!result.Items.Any(i => i.GuestId == item.GuestId && i.GuestId > 0))
                                    {
                                        result.Items.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "PMS integration {IntegrationId} için check-in verileri alınamadı", integration.Id);
                    }
                }

                result.TotalCount = result.Items.Count;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bugünkü check-in'ler getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<ConciergeCheckInOutDto> GetTodayCheckOutsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var result = new ConciergeCheckInOutDto
                {
                    Date = today,
                    Items = new List<CheckInOutItemDto>()
                };

                // GuestFlow'dan bugünkü check-out'ları getir
                var guestFlowCheckOuts = await _guestRepository.GetAll()
                    .Where(g => g.CheckOutDate.HasValue && 
                               g.CheckOutDate.Value.Date == today && 
                               !g.IsDeleted)
                    .Select(g => new CheckInOutItemDto
                    {
                        GuestId = g.Id,
                        GuestName = g.FullName,
                        GuestCode = g.GuestCode,
                        RoomNumber = g.RoomNumber,
                        CheckInDate = g.CheckInDate,
                        CheckOutDate = g.CheckOutDate,
                        Email = g.Email,
                        PhoneNumber = g.PhoneNumber,
                        IsVIP = g.IsSpecialGuest,
                        Source = "GuestFlow"
                    })
                    .ToListAsync();

                result.Items.AddRange(guestFlowCheckOuts);

                // PMS'den bugünkü check-out'ları getir
                var activePMSIntegrations = await _unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                foreach (var integration in activePMSIntegrations)
                {
                    try
                    {
                        // Bugünkü rezervasyonları getir
                        var reservationsResponse = await _pmsIntegrationService.GetReservationsAsync(
                            integration.Id, today.AddDays(-7), today.AddDays(1));

                        if (reservationsResponse.Success && reservationsResponse.Data != null)
                        {
                            foreach (var pmsReservation in reservationsResponse.Data)
                            {
                                // Check-out tarihi bugün mü?
                                if (pmsReservation.CheckOutDate.Date == today)
                                {
                                    // Guest mapping'ini bul
                                    var guestMapping = await _unitOfWork.PMSGuestMappings
                                        .GetAll(m => m.PMSIntegrationId == integration.Id && 
                                                    m.PMSGuestId == pmsReservation.PMSGuestId)
                                        .Include(m => m.GuestFlowGuest)
                                        .FirstOrDefaultAsync();

                                    var item = new CheckInOutItemDto
                                    {
                                        GuestId = guestMapping?.GuestFlowGuestId ?? 0,
                                        GuestName = pmsReservation.GuestName ?? string.Empty,
                                        GuestCode = guestMapping?.GuestFlowGuest?.GuestCode ?? string.Empty,
                                        RoomNumber = pmsReservation.RoomNumber,
                                        RoomType = pmsReservation.RoomType,
                                        CheckInDate = pmsReservation.CheckInDate,
                                        CheckOutDate = pmsReservation.CheckOutDate,
                                        NumberOfGuests = null, // PMSReservation'da NumberOfGuests yok
                                        Email = pmsReservation.GuestEmail,
                                        PhoneNumber = pmsReservation.GuestPhone,
                                        Source = "PMS",
                                        PMSReservationId = pmsReservation.PMSReservationId,
                                        PMSProviderName = integration.ProviderName
                                    };

                                    // GuestFlow'da zaten varsa ekleme (duplicate önleme)
                                    if (!result.Items.Any(i => i.GuestId == item.GuestId && i.GuestId > 0))
                                    {
                                        result.Items.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "PMS integration {IntegrationId} için check-out verileri alınamadı", integration.Id);
                    }
                }

                result.TotalCount = result.Items.Count;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bugünkü check-out'lar getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<List<ActiveGuestDto>> GetActiveGuestsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var result = new List<ActiveGuestDto>();

                // GuestFlow'dan aktif misafirleri getir
                var guestFlowActiveGuests = await _guestRepository.GetAll()
                    .Where(g => g.CheckInDate.HasValue && 
                               g.CheckInDate.Value.Date <= today &&
                               (!g.CheckOutDate.HasValue || g.CheckOutDate.Value.Date >= today) &&
                               !g.IsDeleted)
                    .ToListAsync();

                foreach (var guest in guestFlowActiveGuests)
                {
                    var activeGuest = new ActiveGuestDto
                    {
                        GuestId = guest.Id,
                        GuestName = guest.FullName,
                        GuestCode = guest.GuestCode,
                        RoomNumber = guest.RoomNumber,
                        CheckInDate = guest.CheckInDate,
                        CheckOutDate = guest.CheckOutDate,
                        NumberOfNights = guest.CheckInDate.HasValue && guest.CheckOutDate.HasValue
                            ? (int)(guest.CheckOutDate.Value - guest.CheckInDate.Value).TotalDays
                            : null,
                        Email = guest.Email,
                        PhoneNumber = guest.PhoneNumber,
                        IsVIP = guest.IsSpecialGuest,
                        Source = "GuestFlow"
                    };

                    // Yaklaşan servisleri getir
                    activeGuest.UpcomingServices = await GetUpcomingServicesForGuestAsync(guest.Id);
                    result.Add(activeGuest);
                }

                // PMS'den aktif misafirleri getir
                var activePMSIntegrations = await _unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                foreach (var integration in activePMSIntegrations)
                {
                    try
                    {
                        // Aktif rezervasyonları getir (bugün check-in olan veya daha önce check-in olup henüz check-out olmayan)
                        var reservationsResponse = await _pmsIntegrationService.GetReservationsAsync(
                            integration.Id, today.AddDays(-30), today.AddDays(1));

                        if (reservationsResponse.Success && reservationsResponse.Data != null)
                        {
                            foreach (var pmsReservation in reservationsResponse.Data)
                            {
                                // Aktif rezervasyon mu? (check-in <= today && check-out >= today)
                                if (pmsReservation.CheckInDate.Date <= today &&
                                    pmsReservation.CheckOutDate.Date >= today &&
                                    pmsReservation.Status?.ToLower() != "cancelled")
                                {
                                    // Guest mapping'ini bul
                                    var guestMapping = await _unitOfWork.PMSGuestMappings
                                        .GetAll(m => m.PMSIntegrationId == integration.Id && 
                                                    m.PMSGuestId == pmsReservation.PMSGuestId)
                                        .Include(m => m.GuestFlowGuest)
                                        .FirstOrDefaultAsync();

                                    // GuestFlow'da zaten varsa ekleme (duplicate önleme)
                                    if (guestMapping?.GuestFlowGuestId != null && 
                                        result.Any(r => r.GuestId == guestMapping.GuestFlowGuestId))
                                    {
                                        continue;
                                    }

                                    var activeGuest = new ActiveGuestDto
                                    {
                                        GuestId = guestMapping?.GuestFlowGuestId ?? 0,
                                        GuestName = pmsReservation.GuestName ?? string.Empty,
                                        GuestCode = guestMapping?.GuestFlowGuest?.GuestCode ?? string.Empty,
                                        RoomNumber = pmsReservation.RoomNumber,
                                        RoomType = pmsReservation.RoomType,
                                        CheckInDate = pmsReservation.CheckInDate,
                                        CheckOutDate = pmsReservation.CheckOutDate,
                                        NumberOfNights = (int)(pmsReservation.CheckOutDate - pmsReservation.CheckInDate).TotalDays,
                                        Email = pmsReservation.GuestEmail,
                                        PhoneNumber = pmsReservation.GuestPhone,
                                        Source = "PMS",
                                        PMSReservationId = pmsReservation.PMSReservationId,
                                        PMSProviderName = integration.ProviderName
                                    };

                                    if (guestMapping?.GuestFlowGuestId != null)
                                    {
                                        activeGuest.UpcomingServices = await GetUpcomingServicesForGuestAsync(guestMapping.GuestFlowGuestId);
                                    }

                                    result.Add(activeGuest);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "PMS integration {IntegrationId} için aktif misafir verileri alınamadı", integration.Id);
                    }
                }

                return result.OrderBy(g => g.CheckInDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktif misafirler getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<UnifiedGuestProfileDto> GetUnifiedGuestProfileAsync(int guestId)
        {
            try
            {
                var guest = await _guestRepository.GetByIdAsync(guestId);
                if (guest == null)
                    throw new ArgumentException($"Guest with ID {guestId} not found");

                // Room assignment history
                var roomAssignments = await _roomAssignmentRepository.GetAll()
                    .Where(ra => ra.GuestId == guest.Id && !ra.IsDeleted)
                    .OrderByDescending(ra => ra.StartDate)
                    .Select(ra => new RoomAssignmentHistoryDto
                    {
                        RoomNumber = ra.RoomNumber,
                        StartDate = ra.StartDate,
                        EndDate = ra.EndDate,
                        Source = ra.Source,
                        Notes = ra.Notes
                    })
                    .ToListAsync();

                // Preferences
                var preferences = await _unitOfWork.GuestPreferences
                    .GetAll(p => p.GuestId == guest.Id && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                GuestPreferencesDto? preferencesDto = null;
                if (preferences != null)
                {
                    preferencesDto = new GuestPreferencesDto
                    {
                        PreferredRoomType = preferences.PreferredRoomType,
                        RoomSpecialRequests = preferences.RoomSpecialRequests,
                        BedPreference = preferences.BedPreference,
                        SmokingPreference = preferences.SmokingPreference,
                        DietaryPreferences = preferences.DietaryPreferences,
                        FoodAllergies = preferences.FoodAllergies,
                        ActivityPreferences = preferences.ActivityPreferences,
                        Interests = preferences.Interests,
                        PrefersEmail = preferences.PrefersEmail,
                        PrefersSMS = preferences.PrefersSMS,
                        PrefersWhatsApp = preferences.PrefersWhatsApp,
                        PrefersPhone = preferences.PrefersPhone,
                        PreferredLanguage = preferences.PreferredLanguage,
                        Notes = preferences.Notes,
                        Source = preferences.Source
                    };
                }

                // Invoice history
                var invoices = await _unitOfWork.Invoices
                    .GetAll(i => i.GuestId == guest.Id && !i.IsDeleted)
                    .Include(i => i.InvoiceItems)
                    .OrderByDescending(i => i.IssueDate)
                    .Take(10)
                    .Select(i => new InvoiceSummaryDto
                    {
                        InvoiceId = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        IssueDate = i.IssueDate,
                        TotalAmount = i.TotalAmount,
                        Currency = i.Currency,
                        Status = i.Status.ToString(),
                        ItemCount = i.InvoiceItems.Count(ii => !ii.IsDeleted)
                    })
                    .ToListAsync();

                var result = new UnifiedGuestProfileDto
                {
                    GuestId = guest.Id,
                    GuestName = guest.FullName,
                    GuestCode = guest.GuestCode,
                    GuestFlowData = new GuestFlowDataDto
                    {
                        GuestId = guest.Id,
                        RoomNumber = guest.RoomNumber,
                        CheckInDate = guest.CheckInDate,
                        CheckOutDate = guest.CheckOutDate,
                        Email = guest.Email,
                        PhoneNumber = guest.PhoneNumber,
                        IsVIP = guest.IsSpecialGuest,
                        ServiceHistory = await GetServiceHistoryForGuestAsync(guest.Id),
                        RoomAssignmentHistory = roomAssignments,
                        Preferences = preferencesDto,
                        InvoiceHistory = invoices
                    }
                };

                // PMS verilerini getir
                var pmsMappings = await _unitOfWork.PMSGuestMappings
                    .GetAll(m => m.GuestFlowGuestId == guestId)
                    .Include(m => m.PMSIntegration)
                    .ToListAsync();

                foreach (var mapping in pmsMappings)
                {
                    try
                    {
                        var pmsGuestResponse = await _pmsIntegrationService.GetGuestProfileAsync(
                            mapping.PMSIntegrationId, mapping.PMSGuestId);

                        if (pmsGuestResponse.Success && pmsGuestResponse.Data != null)
                        {
                            var pmsData = new PMSDataDto
                            {
                                ProviderName = mapping.PMSIntegration.ProviderName,
                                ProviderCode = mapping.PMSIntegration.ProviderCode,
                                PMSGuestId = mapping.PMSGuestId,
                                Email = pmsGuestResponse.Data.Email,
                                PhoneNumber = pmsGuestResponse.Data.PhoneNumber,
                                IsVIP = pmsGuestResponse.Data.IsVIP,
                                LastSyncedAt = DateTime.UtcNow
                            };

                            // Reservation bilgilerini de getir (aktif ve geçmiş)
                            var reservationsResponse = await _pmsIntegrationService.GetReservationsAsync(
                                mapping.PMSIntegrationId, DateTime.UtcNow.AddDays(-365), DateTime.UtcNow.AddDays(30));

                            if (reservationsResponse.Success && reservationsResponse.Data != null)
                            {
                                var guestReservations = reservationsResponse.Data
                                    .Where(r => r.PMSGuestId == mapping.PMSGuestId)
                                    .OrderByDescending(r => r.CheckInDate)
                                    .ToList();

                                // Aktif rezervasyon
                                var activeReservation = guestReservations
                                    .FirstOrDefault(r => r.CheckInDate <= DateTime.UtcNow &&
                                                        r.CheckOutDate >= DateTime.UtcNow);

                                if (activeReservation != null)
                                {
                                    pmsData.PMSReservationId = activeReservation.PMSReservationId;
                                    pmsData.RoomNumber = activeReservation.RoomNumber;
                                    pmsData.RoomType = activeReservation.RoomType;
                                    pmsData.CheckInDate = activeReservation.CheckInDate;
                                    pmsData.CheckOutDate = activeReservation.CheckOutDate;
                                }

                                // Reservation history
                                pmsData.ReservationHistory = guestReservations
                                    .Select(r => new PMSReservationHistoryDto
                                    {
                                        PMSReservationId = r.PMSReservationId,
                                        RoomNumber = r.RoomNumber,
                                        RoomType = r.RoomType,
                                        CheckInDate = r.CheckInDate,
                                        CheckOutDate = r.CheckOutDate,
                                        NumberOfNights = (r.CheckOutDate - r.CheckInDate).Days,
                                        Status = r.Status,
                                        TotalAmount = r.TotalAmount,
                                        Currency = r.Currency,
                                        CreatedAt = r.CreatedAt,
                                        LastModifiedAt = r.LastModifiedAt
                                    })
                                    .ToList();
                            }

                            result.PMSData.Add(pmsData);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "PMS guest profile alınamadı: IntegrationId={IntegrationId}, PMSGuestId={PMSGuestId}",
                            mapping.PMSIntegrationId, mapping.PMSGuestId);
                    }
                }

                // Birleşik görünüm - en güncel verileri kullan
                result.RoomNumber = result.PMSData.FirstOrDefault()?.RoomNumber ?? result.GuestFlowData?.RoomNumber;
                result.RoomType = result.PMSData.FirstOrDefault()?.RoomType;
                result.CheckInDate = result.PMSData.FirstOrDefault()?.CheckInDate ?? result.GuestFlowData?.CheckInDate;
                result.CheckOutDate = result.PMSData.FirstOrDefault()?.CheckOutDate ?? result.GuestFlowData?.CheckOutDate;
                result.Email = result.PMSData.FirstOrDefault()?.Email ?? result.GuestFlowData?.Email;
                result.PhoneNumber = result.PMSData.FirstOrDefault()?.PhoneNumber ?? result.GuestFlowData?.PhoneNumber;
                result.IsVIP = result.PMSData.Any(p => p.IsVIP) || (result.GuestFlowData?.IsVIP ?? false);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unified guest profile getirilirken hata oluştu: GuestId={GuestId}", guestId);
                throw;
            }
        }

        public async Task<UpcomingServicesDto> GetUpcomingServicesForTodayAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var result = new UpcomingServicesDto();

                // Transfers (bugün ve yarın)
                var transfers = await _transferRepository.GetAll()
                    .Where(t => (t.TransferDate.Date == today || t.TransferDate.Date == tomorrow) && !t.IsDeleted)
                    .Include(t => t.Guest)
                    .Select(t => new UpcomingServiceItemDto
                    {
                        ServiceId = t.Id,
                        ServiceType = "Transfer",
                        ServiceDate = t.TransferDate,
                        GuestId = t.GuestId,
                        GuestName = t.Guest.FullName,
                        RoomNumber = t.Guest.RoomNumber,
                        CityName = t.DropoffAddress,
                        Status = t.Status.ToString(),
                        IsUrgent = t.TransferDate <= DateTime.UtcNow.AddHours(3)
                    })
                    .ToListAsync();

                // City Tours
                var cityTours = await _cityTourRepository.GetAll()
                    .Where(t => (t.TourDate.Date == today || t.TourDate.Date == tomorrow) && !t.IsDeleted)
                    .Include(t => t.OwnerGuest)
                    .Include(t => t.City)
                    .Select(t => new UpcomingServiceItemDto
                    {
                        ServiceId = t.Id,
                        ServiceType = "CityTour",
                        ServiceDate = t.TourDate,
                        GuestId = t.OwnerGuestId,
                        GuestName = t.OwnerGuest.FullName,
                        RoomNumber = t.OwnerGuest.RoomNumber,
                        CityName = t.City.CityName,
                        Status = t.Status.ToString(),
                        IsUrgent = t.TourDate <= DateTime.UtcNow.AddDays(1)
                    })
                    .ToListAsync();

                // Yacht Tours
                var yachtTours = await _yachtTourRepository.GetAll()
                    .Where(t => (t.TourDate.Date == today || t.TourDate.Date == tomorrow) && !t.IsDeleted)
                    .Include(t => t.OwnerGuest)
                    .Select(t => new UpcomingServiceItemDto
                    {
                        ServiceId = t.Id,
                        ServiceType = "YachtTour",
                        ServiceDate = t.TourDate,
                        GuestId = t.OwnerGuestId,
                        GuestName = t.OwnerGuest.FullName,
                        RoomNumber = t.OwnerGuest.RoomNumber,
                        Status = t.Status.ToString(),
                        IsUrgent = t.TourDate <= DateTime.UtcNow.AddDays(1)
                    })
                    .ToListAsync();

                result.Items = transfers.Concat(cityTours).Concat(yachtTours)
                    .OrderBy(s => s.ServiceDate)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yaklaşan servisler getirilirken hata oluştu");
                throw;
            }
        }

        private async Task<List<UpcomingServiceItemDto>> GetUpcomingServicesForGuestAsync(int guestId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var result = new List<UpcomingServiceItemDto>();

            // Transfers
            var transfers = await _transferRepository.GetAll()
                .Where(t => t.GuestId == guestId && 
                           t.TransferDate.Date >= today && 
                           t.TransferDate.Date <= tomorrow &&
                           !t.IsDeleted)
                .Include(t => t.Guest)
                .Select(t => new UpcomingServiceItemDto
                {
                    ServiceId = t.Id,
                    ServiceType = "Transfer",
                    ServiceDate = t.TransferDate,
                    GuestId = t.GuestId,
                    GuestName = t.Guest.FullName,
                    RoomNumber = t.Guest.RoomNumber,
                    CityName = t.DropoffAddress,
                    Status = t.Status.ToString(),
                    IsUrgent = t.TransferDate <= DateTime.UtcNow.AddHours(3)
                })
                .ToListAsync();

            result.AddRange(transfers);

            // City Tours
            var cityTours = await _cityTourRepository.GetAll()
                .Where(t => t.OwnerGuestId == guestId && 
                           t.TourDate.Date >= today && 
                           t.TourDate.Date <= tomorrow &&
                           !t.IsDeleted)
                .Include(t => t.OwnerGuest)
                .Include(t => t.City)
                .Select(t => new UpcomingServiceItemDto
                {
                    ServiceId = t.Id,
                    ServiceType = "CityTour",
                    ServiceDate = t.TourDate,
                    GuestName = t.OwnerGuest.FullName,
                    RoomNumber = t.OwnerGuest.RoomNumber,
                    CityName = t.City.CityName,
                    Status = t.Status.ToString(),
                    IsUrgent = t.TourDate <= DateTime.UtcNow.AddDays(1)
                })
                .ToListAsync();

            result.AddRange(cityTours);

            // Yacht Tours
            var yachtTours = await _yachtTourRepository.GetAll()
                .Where(t => t.OwnerGuestId == guestId && 
                           t.TourDate.Date >= today && 
                           t.TourDate.Date <= tomorrow &&
                           !t.IsDeleted)
                .Include(t => t.OwnerGuest)
                .Select(t => new UpcomingServiceItemDto
                {
                    ServiceId = t.Id,
                    ServiceType = "YachtTour",
                    ServiceDate = t.TourDate,
                    GuestName = t.OwnerGuest.FullName,
                    RoomNumber = t.OwnerGuest.RoomNumber,
                    Status = t.Status.ToString(),
                    IsUrgent = t.TourDate <= DateTime.UtcNow.AddDays(1)
                })
                .ToListAsync();

            result.AddRange(yachtTours);

            return result.OrderBy(s => s.ServiceDate).ToList();
        }

        private async Task<List<ServiceHistoryDto>> GetServiceHistoryForGuestAsync(int guestId)
        {
            var result = new List<ServiceHistoryDto>();

            // Transfers
            var transfers = await _transferRepository.GetAll()
                .Where(t => t.GuestId == guestId && !t.IsDeleted)
                .OrderByDescending(t => t.TransferDate)
                .Take(10)
                .Select(t => new ServiceHistoryDto
                {
                    ServiceType = "Transfer",
                    ServiceDate = t.TransferDate,
                    Description = $"{t.PickupAddress} → {t.DropoffAddress}",
                    Amount = t.Price,
                    Status = t.Status.ToString()
                })
                .ToListAsync();

            result.AddRange(transfers);

            // City Tours
            var cityTours = await _cityTourRepository.GetAll()
                .Where(t => t.OwnerGuestId == guestId && !t.IsDeleted)
                .OrderByDescending(t => t.TourDate)
                .Take(10)
                .Select(t => new ServiceHistoryDto
                {
                    ServiceType = "CityTour",
                    ServiceDate = t.TourDate,
                    Description = $"City Tour - {t.City.CityName}",
                    Amount = t.FinalPrice,
                    Status = t.Status.ToString()
                })
                .ToListAsync();

            result.AddRange(cityTours);

            // Yacht Tours
            var yachtTours = await _yachtTourRepository.GetAll()
                .Where(t => t.OwnerGuestId == guestId && !t.IsDeleted)
                .OrderByDescending(t => t.TourDate)
                .Take(10)
                .Select(t => new ServiceHistoryDto
                {
                    ServiceType = "YachtTour",
                    ServiceDate = t.TourDate,
                    Description = t.YachtName ?? "Yacht Tour",
                    Amount = t.FinalPrice,
                    Status = t.Status.ToString()
                })
                .ToListAsync();

            result.AddRange(yachtTours);

            return result.OrderByDescending(s => s.ServiceDate).ToList();
        }

        public async Task<GuestHistoryDashboardDto> GetGuestHistoryDashboardAsync(int guestId)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    throw new ArgumentException($"Guest with ID {guestId} not found");

                var result = new GuestHistoryDashboardDto
                {
                    GuestId = guest.Id,
                    GuestName = guest.FullName,
                    GuestCode = guest.GuestCode
                };

                // Önceki konaklamalar (PMS'den)
                var previousStays = new List<PreviousStayDto>();
                var activePMSIntegrations = await _unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                foreach (var integration in activePMSIntegrations)
                {
                    try
                    {
                        // Guest mapping'ini bul
                        var guestMapping = await _unitOfWork.PMSGuestMappings
                            .GetAll(m => m.PMSIntegrationId == integration.Id && m.GuestFlowGuestId == guestId)
                            .FirstOrDefaultAsync();

                        if (guestMapping != null && !string.IsNullOrEmpty(guestMapping.PMSGuestId))
                        {
                            // PMS'den geçmiş rezervasyonları çek (son 2 yıl)
                            var twoYearsAgo = DateTime.UtcNow.AddYears(-2);
                            var reservationsResponse = await _pmsIntegrationService.GetReservationsAsync(
                                integration.Id, twoYearsAgo, DateTime.UtcNow.AddDays(1));

                            if (reservationsResponse.Success && reservationsResponse.Data != null)
                            {
                                foreach (var pmsReservation in reservationsResponse.Data)
                                {
                                    // Sadece check-out yapılmış rezervasyonları al (geçmiş konaklamalar)
                                    if (pmsReservation.CheckOutDate < DateTime.UtcNow)
                                    {
                                        previousStays.Add(new PreviousStayDto
                                        {
                                            PMSReservationId = pmsReservation.PMSReservationId,
                                            PMSProviderName = integration.ProviderName,
                                            RoomNumber = pmsReservation.RoomNumber,
                                            RoomType = pmsReservation.RoomType,
                                            CheckInDate = pmsReservation.CheckInDate,
                                            CheckOutDate = pmsReservation.CheckOutDate,
                                            NumberOfNights = (int)(pmsReservation.CheckOutDate - pmsReservation.CheckInDate).TotalDays,
                                            TotalAmount = pmsReservation.TotalAmount,
                                            Currency = pmsReservation.Currency,
                                            LastSyncedAt = DateTime.UtcNow // PMS reservation'da LastSyncedAt yok, şimdilik UtcNow kullanıyoruz
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "PMS integration {IntegrationId} için geçmiş konaklamalar alınamadı", integration.Id);
                    }
                }

                result.PreviousStays = previousStays.OrderByDescending(s => s.CheckOutDate).ToList();

                // Hizmet geçmişi (GuestFlow'dan)
                result.ServiceHistory = await GetServiceHistoryForGuestAsync(guestId);

                // Harcama analizi
                var spendingAnalysis = new SpendingAnalysisDto
                {
                    Currency = "TRY"
                };

                // GuestFlow harcamaları
                var guestFlowSpending = result.ServiceHistory.Sum(s => s.Amount ?? 0);
                spendingAnalysis.GuestFlowSpending = guestFlowSpending;

                // PMS harcamaları (önceki konaklamalardan)
                var pmsSpending = previousStays.Where(s => s.TotalAmount.HasValue).Sum(s => s.TotalAmount!.Value);
                spendingAnalysis.PMSSpending = pmsSpending > 0 ? pmsSpending : null;

                spendingAnalysis.TotalSpending = guestFlowSpending + (pmsSpending > 0 ? pmsSpending : 0);
                spendingAnalysis.TotalStays = previousStays.Count;
                spendingAnalysis.TotalServices = result.ServiceHistory.Count;
                spendingAnalysis.AverageSpendingPerStay = spendingAnalysis.TotalStays > 0 
                    ? spendingAnalysis.TotalSpending / spendingAnalysis.TotalStays 
                    : 0;
                spendingAnalysis.AverageSpendingPerService = spendingAnalysis.TotalServices > 0 
                    ? guestFlowSpending / spendingAnalysis.TotalServices 
                    : 0;

                // Kategori bazlı harcama
                var spendingByCategory = new List<SpendingByCategoryDto>();
                
                // Accommodation (PMS)
                if (pmsSpending > 0)
                {
                    spendingByCategory.Add(new SpendingByCategoryDto
                    {
                        Category = "Accommodation",
                        Amount = pmsSpending,
                        Count = previousStays.Count
                    });
                }

                // Transfer
                var transferSpending = result.ServiceHistory
                    .Where(s => s.ServiceType == "Transfer")
                    .Sum(s => s.Amount ?? 0);
                if (transferSpending > 0)
                {
                    spendingByCategory.Add(new SpendingByCategoryDto
                    {
                        Category = "Transfer",
                        Amount = transferSpending,
                        Count = result.ServiceHistory.Count(s => s.ServiceType == "Transfer")
                    });
                }

                // Tour
                var tourSpending = result.ServiceHistory
                    .Where(s => s.ServiceType == "CityTour" || s.ServiceType == "YachtTour")
                    .Sum(s => s.Amount ?? 0);
                if (tourSpending > 0)
                {
                    spendingByCategory.Add(new SpendingByCategoryDto
                    {
                        Category = "Tour",
                        Amount = tourSpending,
                        Count = result.ServiceHistory.Count(s => s.ServiceType == "CityTour" || s.ServiceType == "YachtTour")
                    });
                }

                spendingAnalysis.SpendingByCategory = spendingByCategory;
                result.SpendingAnalysis = spendingAnalysis;

                // Tercih analizi
                var preferenceAnalysis = new PreferenceAnalysisDto();

                // Oda tercihleri
                var roomPreferences = previousStays
                    .Where(s => !string.IsNullOrEmpty(s.RoomType))
                    .GroupBy(s => s.RoomType)
                    .Select(g => new RoomPreferenceDto
                    {
                        RoomType = g.Key!,
                        StayCount = g.Count()
                    })
                    .OrderByDescending(r => r.StayCount)
                    .ToList();
                preferenceAnalysis.RoomPreferences = roomPreferences;

                // Servis tercihleri
                var servicePreferences = result.ServiceHistory
                    .GroupBy(s => s.ServiceType)
                    .Select(g => new ServicePreferenceDto
                    {
                        ServiceType = g.Key,
                        UsageCount = g.Count(),
                        TotalSpending = g.Sum(s => s.Amount ?? 0)
                    })
                    .OrderByDescending(s => s.UsageCount)
                    .ToList();
                preferenceAnalysis.ServicePreferences = servicePreferences;

                result.PreferenceAnalysis = preferenceAnalysis;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest history dashboard for guest {GuestId}", guestId);
                throw;
            }
        }

        public async Task<ConciergeDashboardSummaryDto> GetConciergeDashboardSummaryAsync()
        {
            try
            {
                // Daily Operations verilerini al
                var dailyOperations = await _dailyOperationsService.GetDailyOperationsAsync();

                var result = new ConciergeDashboardSummaryDto
                {
                    TodayCheckIns = await GetTodayCheckInsAsync(),
                    TodayCheckOuts = await GetTodayCheckOutsAsync(),
                    ActiveGuests = await GetActiveGuestsAsync(),
                    UpcomingServices = await GetUpcomingServicesForTodayAsync(),
                    GuestStatusIndicators = await GetGuestStatusIndicatorsAsync(),
                    DailyOperations = dailyOperations
                };

                // Quick stats hesapla (Daily Operations'tan + Concierge Dashboard'tan)
                result.QuickStats = new DailyOperationsQuickStatsDto
                {
                    TodayServiceCount = dailyOperations.TodayServices.Count,
                    UpcomingServiceCount = dailyOperations.UpcomingServices.Count,
                    UrgentServiceCount = dailyOperations.TodayServices.Count(s => s.IsUrgent) + 
                                       dailyOperations.UpcomingServices.Count(s => s.IsUrgent),
                    UnassignedDriverCount = dailyOperations.TodayServices.Count(s => !s.AssignedPersonnelId.HasValue) +
                                          dailyOperations.UpcomingServices.Count(s => !s.AssignedPersonnelId.HasValue),
                    UnpaidServiceCount = dailyOperations.TodayServices.Count(s => !s.IsPaid) +
                                        dailyOperations.UpcomingServices.Count(s => !s.IsPaid),
                    OverduePaymentCount = dailyOperations.RiskFlags.Count(r => r.Type == GuestFlow.Application.Operations.Dashboard.Dtos.RiskFlagType.OverduePayment)
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get concierge dashboard summary");
                throw;
            }
        }

        public async Task<List<GuestStatusIndicatorDto>> GetGuestStatusIndicatorsAsync()
        {
            try
            {
                var indicators = new List<GuestStatusIndicatorDto>();
                var today = DateTime.UtcNow.Date;

                // Aktif misafirleri getir
                var activeGuests = await GetActiveGuestsAsync();

                foreach (var guest in activeGuests)
                {
                    // VIP misafirler
                    if (guest.IsVIP)
                    {
                        indicators.Add(new GuestStatusIndicatorDto
                        {
                            GuestId = guest.GuestId,
                            GuestName = guest.GuestName,
                            GuestCode = guest.GuestCode,
                            RoomNumber = guest.RoomNumber,
                            StatusType = GuestStatusType.VIP,
                            Title = "VIP Misafir",
                            Description = "VIP statüsünde misafir"
                        });
                    }

                    // Özel istekleri olan misafirler
                    var guestEntity = await _guestRepository.GetByIdAsync(guest.GuestId);
                    if (guestEntity != null)
                    {
                        var preferences = await _unitOfWork.GuestPreferences
                            .GetAll(p => p.GuestId == guest.GuestId && !p.IsDeleted)
                            .FirstOrDefaultAsync();

                        if (preferences != null)
                        {
                            if (!string.IsNullOrEmpty(preferences.RoomSpecialRequests) ||
                                !string.IsNullOrEmpty(preferences.SpecialFoodRequests) ||
                                !string.IsNullOrEmpty(preferences.FoodAllergies))
                            {
                                var specialRequests = new List<string>();
                                if (!string.IsNullOrEmpty(preferences.RoomSpecialRequests))
                                    specialRequests.Add($"Oda: {preferences.RoomSpecialRequests}");
                                if (!string.IsNullOrEmpty(preferences.SpecialFoodRequests))
                                    specialRequests.Add($"Yemek: {preferences.SpecialFoodRequests}");
                                if (!string.IsNullOrEmpty(preferences.FoodAllergies))
                                    specialRequests.Add($"Alerji: {preferences.FoodAllergies}");

                                indicators.Add(new GuestStatusIndicatorDto
                                {
                                    GuestId = guest.GuestId,
                                    GuestName = guest.GuestName,
                                    GuestCode = guest.GuestCode,
                                    RoomNumber = guest.RoomNumber,
                                    StatusType = GuestStatusType.SpecialRequests,
                                    Title = "Özel İstekler",
                                    Description = string.Join(" | ", specialRequests)
                                });
                            }
                        }

                        // Doğum günü kontrolü (PMS'den çekilebilir, şimdilik GuestFlow'da yok)
                        // TODO: Doğum günü bilgisi PMS'den çekilebilir

                        // Tekrar eden misafirler (PMS'den geçmiş konaklamalar)
                        var pmsMappings = await _unitOfWork.PMSGuestMappings
                            .GetAll(m => m.GuestFlowGuestId == guest.GuestId)
                            .Include(m => m.PMSIntegration)
                            .ToListAsync();

                        foreach (var mapping in pmsMappings)
                        {
                            try
                            {
                                // Son 1 yıl içindeki rezervasyonları getir
                                var reservationsResponse = await _pmsIntegrationService.GetReservationsAsync(
                                    mapping.PMSIntegrationId, 
                                    DateTime.UtcNow.AddYears(-1), 
                                    DateTime.UtcNow);

                                if (reservationsResponse.Success && reservationsResponse.Data != null)
                                {
                                    var guestReservations = reservationsResponse.Data
                                        .Where(r => r.PMSGuestId == mapping.PMSGuestId)
                                        .ToList();

                                    if (guestReservations.Count > 1)
                                    {
                                        indicators.Add(new GuestStatusIndicatorDto
                                        {
                                            GuestId = guest.GuestId,
                                            GuestName = guest.GuestName,
                                            GuestCode = guest.GuestCode,
                                            RoomNumber = guest.RoomNumber,
                                            StatusType = GuestStatusType.RepeatGuest,
                                            Title = "Tekrar Eden Misafir",
                                            Description = $"{guestReservations.Count} konaklama geçmişi ({mapping.PMSIntegration.ProviderName})"
                                        });
                                        break; // Bir PMS'den bulundu, diğerlerini kontrol etme
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "PMS reservation history alınamadı: GuestId={GuestId}, IntegrationId={IntegrationId}",
                                    guest.GuestId, mapping.PMSIntegrationId);
                            }
                        }
                    }
                }

                return indicators.OrderByDescending(i => i.StatusType).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest status indicators");
                throw;
            }
        }
    }
}
