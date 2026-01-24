// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Dashboard.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Dashboard
{
    /// <summary>
    /// Günlük operasyon ekranı için servis
    /// Bugün/yaklaşan servisler, risk bayrakları, hızlı aksiyonlar
    /// </summary>
    public class DailyOperationsService
    {
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly ILogger<DailyOperationsService> _logger;

        public DailyOperationsService(
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<PaymentEntity> paymentRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            ILogger<DailyOperationsService> logger)
        {
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _logger = logger;
        }

        /// <summary>
        /// Günlük operasyon özetini getirir
        /// </summary>
        public async Task<DailyOperationsDto> GetDailyOperationsAsync(DateTime? date = null)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;
                var tomorrow = targetDate.AddDays(1);
                var now = DateTime.UtcNow;

                var result = new DailyOperationsDto
                {
                    Date = targetDate,
                    TodayServices = new List<ServiceOperationDto>(),
                    UpcomingServices = new List<ServiceOperationDto>(),
                    RiskFlags = new List<RiskFlagDto>(),
                    QuickStats = new DailyOperationsQuickStatsDto()
                };

                // Bugünkü servisler
                var todayTransfers = await GetTodayTransfersAsync(targetDate);
                var todayCityTours = await GetTodayCityToursAsync(targetDate);
                var todayYachtTours = await GetTodayYachtToursAsync(targetDate);

                result.TodayServices = todayTransfers
                    .Concat(todayCityTours)
                    .Concat(todayYachtTours)
                    .OrderBy(s => s.ServiceTime)
                    .ToList();

                // Yaklaşan servisler (yarın ve sonrası, 7 gün içinde)
                var upcomingTransfers = await GetUpcomingTransfersAsync(targetDate.AddDays(1), targetDate.AddDays(7));
                var upcomingCityTours = await GetUpcomingCityToursAsync(targetDate.AddDays(1), targetDate.AddDays(7));
                var upcomingYachtTours = await GetUpcomingYachtToursAsync(targetDate.AddDays(1), targetDate.AddDays(7));

                result.UpcomingServices = upcomingTransfers
                    .Concat(upcomingCityTours)
                    .Concat(upcomingYachtTours)
                    .OrderBy(s => s.ServiceTime)
                    .Take(20) // İlk 20 yaklaşan servis
                    .ToList();

                // Risk bayrakları
                result.RiskFlags = await GetRiskFlagsAsync(targetDate, now);

                // Hızlı istatistikler
                result.QuickStats = new DailyOperationsQuickStatsDto
                {
                    TodayServiceCount = result.TodayServices.Count,
                    UpcomingServiceCount = result.UpcomingServices.Count,
                    UrgentServiceCount = result.TodayServices.Count(s => s.IsUrgent) + result.UpcomingServices.Count(s => s.IsUrgent),
                    UnassignedDriverCount = result.TodayServices.Count(s => s.ServiceType == "Transfer" && string.IsNullOrEmpty(s.AssignedPersonnelName)),
                    UnpaidServiceCount = result.RiskFlags.Count(r => r.Type == RiskFlagType.UnpaidService),
                    OverduePaymentCount = result.RiskFlags.Count(r => r.Type == RiskFlagType.OverduePayment)
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Günlük operasyon verileri getirilirken hata oluştu: Date={Date}", date);
                throw;
            }
        }

        private async Task<List<ServiceOperationDto>> GetTodayTransfersAsync(DateTime date)
        {
            var transfers = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Include(t => t.Personnel)
                .Include(t => t.PickupCity)
                .Where(t => !t.IsDeleted && t.TransferDate.Date == date)
                .ToListAsync();

            return transfers.Select(t => new ServiceOperationDto
            {
                ServiceId = t.Id,
                ServiceType = "Transfer",
                ServiceTime = t.TransferDate,
                GuestId = t.GuestId,
                GuestName = t.Guest?.FullName ?? "Bilinmiyor",
                GuestCode = t.Guest?.GuestCode ?? "",
                RoomNumber = t.Guest?.RoomNumber ?? "",
                Location = $"{t.PickupAddress} → {t.DropoffAddress}",
                CityName = t.PickupCity?.CityName,
                Status = t.Status ?? "Pending",
                AssignedPersonnelId = t.PersonnelId,
                AssignedPersonnelName = t.Personnel?.FullName,
                Amount = t.FinalPrice,
                Currency = t.Currency ?? "TRY",
                IsUrgent = t.TransferDate <= DateTime.UtcNow.AddHours(2) && t.Status != "Completed",
                IsPaid = false, // Will be calculated separately if needed
                Notes = t.Note
            }).ToList();
        }

        private async Task<List<ServiceOperationDto>> GetTodayCityToursAsync(DateTime date)
        {
            var tours = await _cityTourRepository.GetAll()
                .Include(t => t.OwnerGuest)
                .Include(t => t.City)
                .Where(t => !t.IsDeleted && t.TourDate.Date == date)
                .ToListAsync();

            return tours.Select(t => new ServiceOperationDto
            {
                ServiceId = t.Id,
                ServiceType = "CityTour",
                ServiceTime = t.TourDate,
                GuestId = t.OwnerGuestId,
                GuestName = t.OwnerGuest?.FullName ?? "Bilinmiyor",
                GuestCode = t.OwnerGuest?.GuestCode ?? "",
                RoomNumber = t.OwnerGuest?.RoomNumber ?? "",
                Location = t.City?.CityName ?? "",
                CityName = t.City?.CityName,
                Status = "Scheduled",
                Amount = t.Price,
                Currency = t.Currency ?? "TRY",
                IsUrgent = t.TourDate <= DateTime.UtcNow.AddHours(2),
                Notes = null // CityTourEntity doesn't have Note property
            }).ToList();
        }

        private async Task<List<ServiceOperationDto>> GetTodayYachtToursAsync(DateTime date)
        {
            var tours = await _yachtTourRepository.GetAll()
                .Include(t => t.OwnerGuest)
                .Where(t => !t.IsDeleted && t.TourDate.Date == date)
                .ToListAsync();

            return tours.Select(t => new ServiceOperationDto
            {
                ServiceId = t.Id,
                ServiceType = "YachtTour",
                ServiceTime = t.TourDate,
                GuestId = t.OwnerGuestId,
                GuestName = t.OwnerGuest?.FullName ?? "Bilinmiyor",
                GuestCode = t.OwnerGuest?.GuestCode ?? "",
                RoomNumber = t.OwnerGuest?.RoomNumber ?? "",
                Location = "Yacht Tour",
                Status = "Scheduled",
                Amount = t.Price,
                Currency = t.Currency ?? "TRY",
                IsUrgent = t.TourDate <= DateTime.UtcNow.AddHours(2),
                Notes = null // YachtTourEntity doesn't have Note property
            }).ToList();
        }

        private async Task<List<ServiceOperationDto>> GetUpcomingTransfersAsync(DateTime startDate, DateTime endDate)
        {
            var transfers = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Include(t => t.Personnel)
                .Include(t => t.PickupCity)
                .Where(t => !t.IsDeleted && t.TransferDate >= startDate && t.TransferDate <= endDate)
                .OrderBy(t => t.TransferDate)
                .Take(20)
                .ToListAsync();

            return transfers.Select(t => new ServiceOperationDto
            {
                ServiceId = t.Id,
                ServiceType = "Transfer",
                ServiceTime = t.TransferDate,
                GuestId = t.GuestId,
                GuestName = t.Guest?.FullName ?? "Bilinmiyor",
                GuestCode = t.Guest?.GuestCode ?? "",
                RoomNumber = t.Guest?.RoomNumber ?? "",
                Location = $"{t.PickupAddress} → {t.DropoffAddress}",
                CityName = t.PickupCity?.CityName,
                Status = t.Status ?? "Pending",
                AssignedPersonnelId = t.PersonnelId,
                AssignedPersonnelName = t.Personnel?.FullName,
                Amount = t.FinalPrice,
                Currency = t.Currency ?? "TRY",
                IsUrgent = t.TransferDate <= DateTime.UtcNow.AddDays(1),
                IsPaid = false, // Will be calculated separately if needed
                Notes = t.Note
            }).ToList();
        }

        private async Task<List<ServiceOperationDto>> GetUpcomingCityToursAsync(DateTime startDate, DateTime endDate)
        {
            var tours = await _cityTourRepository.GetAll()
                .Include(t => t.OwnerGuest)
                .Include(t => t.City)
                .Where(t => !t.IsDeleted && t.TourDate >= startDate && t.TourDate <= endDate)
                .OrderBy(t => t.TourDate)
                .Take(10)
                .ToListAsync();

            return tours.Select(t => new ServiceOperationDto
            {
                ServiceId = t.Id,
                ServiceType = "CityTour",
                ServiceTime = t.TourDate,
                GuestId = t.OwnerGuestId,
                GuestName = t.OwnerGuest?.FullName ?? "Bilinmiyor",
                GuestCode = t.OwnerGuest?.GuestCode ?? "",
                RoomNumber = t.OwnerGuest?.RoomNumber ?? "",
                Location = t.City?.CityName ?? "",
                CityName = t.City?.CityName,
                Status = "Scheduled",
                Amount = t.Price,
                Currency = t.Currency ?? "TRY",
                IsUrgent = t.TourDate <= DateTime.UtcNow.AddDays(1),
                Notes = null // CityTourEntity doesn't have Note property
            }).ToList();
        }

        private async Task<List<ServiceOperationDto>> GetUpcomingYachtToursAsync(DateTime startDate, DateTime endDate)
        {
            var tours = await _yachtTourRepository.GetAll()
                .Include(t => t.OwnerGuest)
                .Where(t => !t.IsDeleted && t.TourDate >= startDate && t.TourDate <= endDate)
                .OrderBy(t => t.TourDate)
                .Take(10)
                .ToListAsync();

            return tours.Select(t => new ServiceOperationDto
            {
                ServiceId = t.Id,
                ServiceType = "YachtTour",
                ServiceTime = t.TourDate,
                GuestId = t.OwnerGuestId,
                GuestName = t.OwnerGuest?.FullName ?? "Bilinmiyor",
                GuestCode = t.OwnerGuest?.GuestCode ?? "",
                RoomNumber = t.OwnerGuest?.RoomNumber ?? "",
                Location = "Yacht Tour",
                Status = "Scheduled",
                Amount = t.Price,
                Currency = t.Currency ?? "TRY",
                IsUrgent = t.TourDate <= DateTime.UtcNow.AddDays(1),
                Notes = null // YachtTourEntity doesn't have Note property
            }).ToList();
        }

        private async Task<List<RiskFlagDto>> GetRiskFlagsAsync(DateTime date, DateTime now)
        {
            var riskFlags = new List<RiskFlagDto>();

            // Geciken ödemeler (30 günden eski, ödenmemiş)
            var overduePayments = await _paymentRepository.GetAll()
                .Include(p => p.Invoice)
                .Where(p => !p.IsDeleted && 
                           p.PaymentDate < now.AddDays(-30) && 
                           p.Status != PaymentStatus.Completed)
                .Select(p => new RiskFlagDto
                {
                    Type = RiskFlagType.OverduePayment,
                    Severity = RiskFlagSeverity.High,
                    Title = "Geciken Ödeme",
                    Description = $"Fatura #{p.InvoiceId ?? 0} - {p.Amount} {p.Currency}",
                    ServiceId = p.InvoiceId ?? 0,
                    ServiceType = "Invoice",
                    CreatedDate = p.PaymentDate
                })
                .Take(10)
                .ToListAsync();

            riskFlags.AddRange(overduePayments);

            // Ödemesi alınmamış bugünkü servisler
            var todayTransfers = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Where(t => !t.IsDeleted && t.TransferDate.Date == date)
                .ToListAsync();

            var unpaidTodayServices = new List<RiskFlagDto>();
            foreach (var transfer in todayTransfers)
            {
                var isPaid = await IsTransferPaidAsync(transfer.Id);
                if (!isPaid)
                {
                    unpaidTodayServices.Add(new RiskFlagDto
                    {
                        Type = RiskFlagType.UnpaidService,
                        Severity = RiskFlagSeverity.Medium,
                        Title = "Ödemesi Alınmamış Transfer",
                        Description = $"{transfer.Guest?.FullName ?? "Bilinmiyor"} - {transfer.FinalPrice} {transfer.Currency ?? "TRY"}",
                        ServiceId = transfer.Id,
                        ServiceType = "Transfer",
                        CreatedDate = transfer.TransferDate
                    });
                }
            }
            riskFlags.AddRange(unpaidTodayServices.Take(10));

            // Atanmayan şoför (bugünkü transferler)
            var unassignedDrivers = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Where(t => !t.IsDeleted && 
                           t.TransferDate.Date == date && 
                           t.PersonnelId == null)
                .Select(t => new RiskFlagDto
                {
                    Type = RiskFlagType.UnassignedDriver,
                    Severity = RiskFlagSeverity.High,
                    Title = "Atanmayan Şoför",
                    Description = $"{t.Guest.FullName} - {t.PickupAddress} → {t.DropoffAddress}",
                    ServiceId = t.Id,
                    ServiceType = "Transfer",
                    CreatedDate = t.TransferDate
                })
                .Take(10)
                .ToListAsync();

            riskFlags.AddRange(unassignedDrivers);

            // Yaklaşan servisler (2 saat içinde, onaylanmamış)
            var urgentUnconfirmed = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Where(t => !t.IsDeleted && 
                           t.TransferDate >= now && 
                           t.TransferDate <= now.AddHours(2) &&
                           t.Status != "Confirmed" &&
                           t.Status != "Completed")
                .Select(t => new RiskFlagDto
                {
                    Type = RiskFlagType.UrgentUnconfirmed,
                    Severity = RiskFlagSeverity.Critical,
                    Title = "Acil Onay Bekleyen Servis",
                    Description = $"{t.Guest.FullName} - {t.TransferDate:HH:mm}",
                    ServiceId = t.Id,
                    ServiceType = "Transfer",
                    CreatedDate = t.TransferDate
                })
                .Take(10)
                .ToListAsync();

            riskFlags.AddRange(urgentUnconfirmed);

            return riskFlags.OrderByDescending(r => r.Severity).ThenBy(r => r.CreatedDate).ToList();
        }

        private async Task<bool> IsTransferPaidAsync(int transferId)
        {
            var payments = await _paymentRepository.GetAll()
                .Where(p => !p.IsDeleted && 
                           p.TransferId == transferId && 
                           p.Status == PaymentStatus.Completed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var transfer = await _transferRepository.GetByIdAsync(transferId, includeDeleted: false);
            if (transfer == null) return false;

            return payments >= transfer.FinalPrice;
        }
    }
}
