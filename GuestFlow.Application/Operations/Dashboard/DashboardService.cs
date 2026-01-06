using GuestFlow.Application.Operations.Payment;
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
    /// Dashboard servisi - Tahsilat bazlı gelir ve ödeme durumu hesaplaması
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<VehicleEntity> _vehicleRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<VehicleEntity> vehicleRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<PaymentEntity> paymentRepository,
            IPaymentStatusService paymentStatusService,
            ILogger<DashboardService> logger)
        {
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _cityRepository = cityRepository;
            _vehicleRepository = vehicleRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _transferRepository = transferRepository;
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _paymentStatusService = paymentStatusService;
            _logger = logger;
        }

        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var lastMonthStart = monthStart.AddMonths(-1);
                var lastMonthEnd = monthStart.AddDays(-1);
                var yearStart = new DateTime(today.Year, 1, 1);

                // Toplam sayılar
                var totalGuests = await _guestRepository.GetAll().CountAsync();
                var totalPersonnel = await _personnelRepository.GetAll().CountAsync();
                var totalCities = await _cityRepository.GetAll().CountAsync();
                var totalVehicles = await _vehicleRepository.GetAll().CountAsync();

                // Bugünkü gelir
                var todayRevenue = await GetRevenueForDateRangeAsync(today, today);

                // Bu haftanın geliri
                var thisWeekRevenue = await GetRevenueForDateRangeAsync(weekStart, today);

                // Bu ayın geliri
                var thisMonthRevenue = await GetRevenueForDateRangeAsync(monthStart, today);

                // Geçen ayın geliri
                var lastMonthRevenue = await GetRevenueForDateRangeAsync(lastMonthStart, lastMonthEnd);

                // Yıl başından bugüne gelir
                var yearToDateRevenue = await GetRevenueForDateRangeAsync(yearStart, today);

                // Aktif rezervasyonlar
                var activeTransfers = await _transferRepository.GetAll()
                    .Where(t => t.TransferDate >= today).CountAsync();

                var upcomingTours = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate >= today).CountAsync() +
                    await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate >= today).CountAsync();

                var pendingInvoices = await _invoiceRepository.GetAll()
                    .Where(i => string.IsNullOrEmpty(i.PdfUrl)).CountAsync();

                var todayBookings = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date == today).CountAsync() +
                    await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date == today).CountAsync() +
                    await _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date == today).CountAsync();

                // Bu ayın rezervasyon sayısı
                var totalBookingsThisMonth = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date >= monthStart && ct.TourDate.Date <= today).CountAsync() +
                    await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date >= monthStart && yt.TourDate.Date <= today).CountAsync() +
                    await _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date >= monthStart && t.TransferDate.Date <= today).CountAsync();

                // Geçen ayın rezervasyon sayısı
                var totalBookingsLastMonth = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date >= lastMonthStart && ct.TourDate.Date <= lastMonthEnd).CountAsync() +
                    await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date >= lastMonthStart && yt.TourDate.Date <= lastMonthEnd).CountAsync() +
                    await _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date >= lastMonthStart && t.TransferDate.Date <= lastMonthEnd).CountAsync();

                // Ortalama rezervasyon değeri
                var averageBookingValue = totalBookingsThisMonth > 0 ? thisMonthRevenue / totalBookingsThisMonth : 0;

                // Gelir büyüme yüzdesi
                var revenueGrowthPercentage = lastMonthRevenue > 0
                    ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
                    : 0;

                // Son rezervasyonlar
                var recentBookings = await GetRecentBookingsAsync(10);

                // Popüler hizmetler
                var popularServices = await GetPopularServicesAsync();

                return new DashboardOverviewDto
                {
                    TotalGuests = totalGuests,
                    TotalPersonnel = totalPersonnel,
                    TotalCities = totalCities,
                    TotalVehicles = totalVehicles,
                    TodayRevenue = todayRevenue,
                    ThisWeekRevenue = thisWeekRevenue,
                    ThisMonthRevenue = thisMonthRevenue,
                    LastMonthRevenue = lastMonthRevenue,
                    YearToDateRevenue = yearToDateRevenue,
                    ActiveTransfers = activeTransfers,
                    UpcomingTours = upcomingTours,
                    PendingInvoices = pendingInvoices,
                    TodayBookings = todayBookings,
                    AverageBookingValue = averageBookingValue,
                    TotalBookingsThisMonth = totalBookingsThisMonth,
                    TotalBookingsLastMonth = totalBookingsLastMonth,
                    RevenueGrowthPercentage = revenueGrowthPercentage,
                    RecentBookings = recentBookings,
                    PopularServices = popularServices
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dashboard özeti getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<QuickStatsDto> GetQuickStatsAsync()
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                // PERFORMANCE: Execute all count queries in parallel to reduce database round trips
                var countTasks = new[]
                {
                    _guestRepository.GetAll().CountAsync(),
                    _personnelRepository.GetAll().CountAsync(),
                    _transferRepository.GetAll().CountAsync(),
                    _cityTourRepository.GetAll().CountAsync(),
                    _yachtTourRepository.GetAll().CountAsync(),
                    _invoiceRepository.GetAll().CountAsync()
                };

                var counts = await Task.WhenAll(countTasks);

                // PERFORMANCE: Optimized active guests query - use UNION instead of OR with EXISTS
                var activeGuestsQuery = await Task.WhenAll(
                    _transferRepository.GetAll()
                        .Where(t => t.TransferDate >= thirtyDaysAgo)
                        .Select(t => t.GuestId)
                        .Distinct()
                        .CountAsync(),

                    _cityTourRepository.GetAll()
                        .Where(ct => ct.TourDate >= thirtyDaysAgo)
                        .Select(ct => ct.OwnerGuestId)
                        .Distinct()
                        .CountAsync(),

                    _yachtTourRepository.GetAll()
                        .Where(yt => yt.TourDate >= thirtyDaysAgo)
                        .Select(yt => yt.OwnerGuestId)
                        .Distinct()
                        .CountAsync()
                );

                var activeGuests = activeGuestsQuery.Sum();

                // REVENUE REALITY: Revenue = collected money only (from PaymentEntity)
                // PERFORMANCE: Use indexed query for completed payments
                var totalRevenue = await _paymentRepository.GetAll()
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                return new QuickStatsDto
                {
                    TotalGuests = counts[0],
                    ActiveGuests = activeGuests,
                    TotalPersonnel = counts[1],
                    TotalTransfers = counts[2],
                    TotalCityTours = counts[3],
                    TotalYachtTours = counts[4],
                    TotalInvoices = counts[5],
                    TotalRevenue = totalRevenue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Hızlı istatistikler getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<RecentActivityDto> GetRecentActivitiesAsync(int? limit = 10)
        {
            try
            {
                var limitValue = limit ?? 10;

                var recentBookings = await GetRecentBookingsAsync(limitValue);

                var recentGuests = await _guestRepository.GetAll()
                    .OrderByDescending(g => g.CreatedDate)
                    .Take(limitValue)
                    .Select(g => new RecentGuestDto
                    {
                        Id = g.Id,
                        FullName = g.FullName,
                        GuestCode = g.GuestCode,
                        Email = g.Email,
                        Nationality = g.Nationality,
                        IsSpecialGuest = g.IsSpecialGuest,
                        CreatedDate = g.CreatedDate
                    })
                    .ToListAsync();

                var recentInvoices = await _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .OrderByDescending(i => i.CreatedDate)
                    .Take(limitValue)
                    .Select(i => new RecentInvoiceDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        GuestName = i.Guest != null ? i.Guest.FullName : "Bilinmiyor",
                        TotalAmount = i.TotalAmount,
                        Currency = i.Currency,
                        IssueDate = i.IssueDate,
                        HasPdf = !string.IsNullOrEmpty(i.PdfUrl),
                        CreatedDate = i.CreatedDate
                    })
                    .ToListAsync();

                return new RecentActivityDto
                {
                    RecentBookings = recentBookings,
                    RecentGuests = recentGuests,
                    RecentInvoices = recentInvoices
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Son aktiviteler getirilirken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Geliri tahsilat bazlı hesaplar (PaymentEntity'den)
        /// Gelir = Tamamlanmış ödemeler (Status = Completed), PaymentDate bazlı
        /// </summary>
        private async Task<decimal> GetRevenueForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Tahsilat bazlı gelir (PaymentEntity'den)
            var completedPayments = await _paymentRepository.GetAll()
                .Where(p => p.PaymentDate.Date >= startDate.Date && 
                           p.PaymentDate.Date <= endDate.Date && 
                           p.Status == PaymentStatus.Completed && 
                           !p.IsDeleted)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // İade edilen tutarları çıkar
            var refundedPayments = await _paymentRepository.GetAll()
                .Where(p => p.RefundDate.HasValue &&
                           p.RefundDate.Value.Date >= startDate.Date && 
                           p.RefundDate.Value.Date <= endDate.Date && 
                           p.Status == PaymentStatus.Refunded && 
                           !p.IsDeleted)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return completedPayments - refundedPayments;
        }

        /// <summary>
        /// Currency bazlı geliri hesaplar
        /// </summary>
        private async Task<Dictionary<string, decimal>> GetRevenueForDateRangeByCurrencyAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _paymentRepository.GetAll()
                .Where(p => p.PaymentDate.Date >= startDate.Date && 
                           p.PaymentDate.Date <= endDate.Date && 
                           p.Status == PaymentStatus.Completed && 
                           !p.IsDeleted)
                .GroupBy(p => p.Currency)
                .Select(g => new { Currency = g.Key, Total = g.Sum(p => p.Amount) })
                .ToListAsync();

            var refunds = await _paymentRepository.GetAll()
                .Where(p => p.RefundDate.HasValue &&
                           p.RefundDate.Value.Date >= startDate.Date && 
                           p.RefundDate.Value.Date <= endDate.Date && 
                           p.Status == PaymentStatus.Refunded && 
                           !p.IsDeleted)
                .GroupBy(p => p.Currency)
                .Select(g => new { Currency = g.Key, Total = g.Sum(p => p.Amount) })
                .ToListAsync();

            var result = payments.ToDictionary(x => x.Currency, x => x.Total);
            
            foreach (var refund in refunds)
            {
                if (result.ContainsKey(refund.Currency))
                    result[refund.Currency] -= refund.Total;
                else
                    result[refund.Currency] = -refund.Total;
            }

            return result;
        }

        private async Task<List<RecentBookingDto>> GetRecentBookingsAsync(int limit)
        {
            var recentCityTours = await _cityTourRepository.GetAll()
                .Include(ct => ct.OwnerGuest)
                .OrderByDescending(ct => ct.CreatedDate)
                .Take(limit)
                .Select(ct => new RecentBookingDto
                {
                    Id = ct.Id,
                    Type = "CityTour",
                    GuestName = ct.OwnerGuest != null ? ct.OwnerGuest.FullName : "Bilinmiyor",
                    GuestCode = ct.OwnerGuest != null ? ct.OwnerGuest.GuestCode : "",
                    BookingDate = ct.TourDate,
                    Amount = ct.FinalPrice,
                    Status = "Aktif",
                    CreatedDate = ct.CreatedDate
                })
                .ToListAsync();

            var recentYachtTours = await _yachtTourRepository.GetAll()
                .Include(yt => yt.OwnerGuest)
                .OrderByDescending(yt => yt.CreatedDate)
                .Take(limit)
                .Select(yt => new RecentBookingDto
                {
                    Id = yt.Id,
                    Type = "YachtTour",
                    GuestName = yt.OwnerGuest != null ? yt.OwnerGuest.FullName : "Bilinmiyor",
                    GuestCode = yt.OwnerGuest != null ? yt.OwnerGuest.GuestCode : "",
                    BookingDate = yt.TourDate,
                    Amount = yt.FinalPrice,
                    Status = "Aktif",
                    CreatedDate = yt.CreatedDate
                })
                .ToListAsync();

            var recentTransfers = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .OrderByDescending(t => t.CreatedDate)
                .Take(limit)
                .Select(t => new RecentBookingDto
                {
                    Id = t.Id,
                    Type = "Transfer",
                    GuestName = t.Guest != null ? t.Guest.FullName : "Bilinmiyor",
                    GuestCode = t.Guest != null ? t.Guest.GuestCode : "",
                    BookingDate = t.TransferDate,
                    Amount = t.FinalPrice,
                    Status = t.Status.ToString(),
                    CreatedDate = t.CreatedDate
                })
                .ToListAsync();

            return recentCityTours
                .Concat(recentYachtTours)
                .Concat(recentTransfers)
                .OrderByDescending(b => b.CreatedDate)
                .Take(limit)
                .ToList();
        }

        private async Task<List<PopularServiceDto>> GetPopularServicesAsync()
        {
            // REVENUE REALITY: Revenue = collected money only (from PaymentEntity)
            // PERFORMANCE: Execute all queries in parallel to reduce database round trips

            // Get booking counts for all services
            var countTasks = new[]
            {
                _transferRepository.GetAll().CountAsync(),
                _cityTourRepository.GetAll().CountAsync(),
                _yachtTourRepository.GetAll().CountAsync()
            };

            // Get revenue sums for all services
            var revenueTasks = new[]
            {
                _paymentRepository.GetAll()
                    .Where(p => p.TransferId.HasValue && p.Status == PaymentStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount),

                _paymentRepository.GetAll()
                    .Where(p => p.CityTourId.HasValue && p.Status == PaymentStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount),

                _paymentRepository.GetAll()
                    .Where(p => p.YachtTourId.HasValue && p.Status == PaymentStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount)
            };

            // Get average prices for all services
            var avgPriceTasks = new[]
            {
                _transferRepository.GetAll()
                    .AverageAsync(t => (decimal?)t.FinalPrice),

                _cityTourRepository.GetAll()
                    .AverageAsync(ct => (decimal?)ct.FinalPrice),

                _yachtTourRepository.GetAll()
                    .AverageAsync(yt => (decimal?)yt.FinalPrice)
            };

            // Execute all queries in parallel
            var counts = await Task.WhenAll(countTasks);
            var revenues = await Task.WhenAll(revenueTasks);
            var avgPrices = await Task.WhenAll(avgPriceTasks);

            var services = new List<PopularServiceDto>
            {
                new PopularServiceDto
                {
                    ServiceType = "Transfer",
                    BookingCount = counts[0],
                    TotalRevenue = revenues[0] ?? 0,
                    AveragePrice = avgPrices[0] ?? 0
                },
                new PopularServiceDto
                {
                    ServiceType = "CityTour",
                    BookingCount = counts[1],
                    TotalRevenue = revenues[1] ?? 0,
                    AveragePrice = avgPrices[1] ?? 0
                },
                new PopularServiceDto
                {
                    ServiceType = "YachtTour",
                    BookingCount = counts[2],
                    TotalRevenue = revenues[2] ?? 0,
                    AveragePrice = avgPrices[2] ?? 0
                }
            };

            return services.OrderByDescending(s => s.BookingCount).ToList();
        }

        public async Task<RevenueChartDataDto> GetRevenueChartDataAsync(string period = "daily", int? days = null)
        {
            try
            {
                var result = new RevenueChartDataDto { Period = period };
                var today = DateTime.UtcNow.Date;

                switch (period.ToLower())
                {
                    case "daily":
                        var dailyDays = days ?? 30; // Son 30 gün
                        var dailyStartDate = today.AddDays(-dailyDays);
                        
                        for (var date = dailyStartDate; date <= today; date = date.AddDays(1))
                        {
                            var revenue = await GetRevenueForDateRangeAsync(date, date);
                            var bookingCount = await GetBookingCountForDateAsync(date);
                            
                            result.Data.Add(new RevenueChartItemDto
                            {
                                Label = date.ToString("dd.MM.yyyy"),
                                Revenue = revenue,
                                BookingCount = bookingCount,
                                Date = date
                            });
                        }
                        break;

                    case "weekly":
                        var weeklyWeeks = days ?? 12; // Son 12 hafta
                        var weekStart = today.AddDays(-(int)today.DayOfWeek);
                        
                        for (int i = weeklyWeeks - 1; i >= 0; i--)
                        {
                            var weekStartDate = weekStart.AddDays(-(i * 7));
                            var weekEndDate = weekStartDate.AddDays(6);
                            if (weekEndDate > today) weekEndDate = today;
                            
                            var revenue = await GetRevenueForDateRangeAsync(weekStartDate, weekEndDate);
                            var bookingCount = await GetBookingCountForDateRangeAsync(weekStartDate, weekEndDate);
                            
                            result.Data.Add(new RevenueChartItemDto
                            {
                                Label = $"{weekStartDate:dd.MM} - {weekEndDate:dd.MM.yyyy}",
                                Revenue = revenue,
                                BookingCount = bookingCount,
                                Date = weekStartDate
                            });
                        }
                        break;

                    case "monthly":
                        var monthlyMonths = days ?? 12; // Son 12 ay
                        var monthStart = new DateTime(today.Year, today.Month, 1);
                        
                        for (int i = monthlyMonths - 1; i >= 0; i--)
                        {
                            var monthStartDate = monthStart.AddMonths(-i);
                            var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
                            if (monthEndDate > today) monthEndDate = today;
                            
                            var revenue = await GetRevenueForDateRangeAsync(monthStartDate, monthEndDate);
                            var bookingCount = await GetBookingCountForDateRangeAsync(monthStartDate, monthEndDate);
                            
                            result.Data.Add(new RevenueChartItemDto
                            {
                                Label = monthStartDate.ToString("MMM yyyy"),
                                Revenue = revenue,
                                BookingCount = bookingCount,
                                Date = monthStartDate
                            });
                        }
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Gelir grafik verileri getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<UpcomingBookingsDto> GetUpcomingBookingsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var start = startDate?.Date ?? today;
                var end = endDate?.Date ?? today.AddDays(30); // Varsayılan: 30 gün ileri
                var weekEnd = today.AddDays(7);
                var monthEnd = today.AddMonths(1);

                // Bugünkü rezervasyonlar
                var todayTransfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Where(t => t.TransferDate.Date == today)
                    .Select(t => new UpcomingBookingDto
                    {
                        Id = t.Id,
                        Type = "Transfer",
                        GuestName = t.Guest != null ? t.Guest.FullName : "Bilinmiyor",
                        GuestCode = t.Guest != null ? t.Guest.GuestCode : "",
                        BookingDate = t.TransferDate,
                        Location = $"{t.PickupAddress} → {t.DropoffAddress}",
                        Description = t.Note ?? "Transfer",
                        Amount = t.FinalPrice,
                        Status = t.Status.ToString(),
                        PersonnelId = t.PersonnelId,
                        PersonnelName = t.Personnel != null ? t.Personnel.FullName : ""
                    })
                    .ToListAsync();

                var todayCityTours = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Include(ct => ct.Personnel)
                    .Include(ct => ct.City)
                    .Where(ct => ct.TourDate.Date == today)
                    .Select(ct => new UpcomingBookingDto
                    {
                        Id = ct.Id,
                        Type = "CityTour",
                        GuestName = ct.OwnerGuest != null ? ct.OwnerGuest.FullName : "Bilinmiyor",
                        GuestCode = ct.OwnerGuest != null ? ct.OwnerGuest.GuestCode : "",
                        BookingDate = ct.TourDate,
                        Location = ct.City != null ? ct.City.CityName : "Bilinmiyor",
                        Description = $"Şehir Turu - {ct.DurationHours} saat",
                        Amount = ct.FinalPrice,
                        Status = "Aktif",
                        PersonnelId = ct.PersonnelId,
                        PersonnelName = ct.Personnel != null ? ct.Personnel.FullName : ""
                    })
                    .ToListAsync();

                var todayYachtTours = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Include(yt => yt.Personnel)
                    .Include(yt => yt.City)
                    .Where(yt => yt.TourDate.Date == today)
                    .Select(yt => new UpcomingBookingDto
                    {
                        Id = yt.Id,
                        Type = "YachtTour",
                        GuestName = yt.OwnerGuest != null ? yt.OwnerGuest.FullName : "Bilinmiyor",
                        GuestCode = yt.OwnerGuest != null ? yt.OwnerGuest.GuestCode : "",
                        BookingDate = yt.TourDate,
                        Location = yt.City != null ? yt.City.CityName : "Bilinmiyor",
                        Description = $"Yat Turu - {yt.YachtName}",
                        Amount = yt.FinalPrice,
                        Status = "Aktif",
                        PersonnelId = yt.PersonnelId,
                        PersonnelName = yt.Personnel != null ? yt.Personnel.FullName : ""
                    })
                    .ToListAsync();

                // Bu haftanın rezervasyonları
                var weekTransfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Where(t => t.TransferDate.Date > today && t.TransferDate.Date <= weekEnd)
                    .Select(t => new UpcomingBookingDto
                    {
                        Id = t.Id,
                        Type = "Transfer",
                        GuestName = t.Guest != null ? t.Guest.FullName : "Bilinmiyor",
                        GuestCode = t.Guest != null ? t.Guest.GuestCode : "",
                        BookingDate = t.TransferDate,
                        Location = $"{t.PickupAddress} → {t.DropoffAddress}",
                        Description = t.Note ?? "Transfer",
                        Amount = t.FinalPrice,
                        Status = t.Status.ToString(),
                        PersonnelId = t.PersonnelId,
                        PersonnelName = t.Personnel != null ? t.Personnel.FullName : ""
                    })
                    .ToListAsync();

                var weekCityTours = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Include(ct => ct.Personnel)
                    .Include(ct => ct.City)
                    .Where(ct => ct.TourDate.Date > today && ct.TourDate.Date <= weekEnd)
                    .Select(ct => new UpcomingBookingDto
                    {
                        Id = ct.Id,
                        Type = "CityTour",
                        GuestName = ct.OwnerGuest != null ? ct.OwnerGuest.FullName : "Bilinmiyor",
                        GuestCode = ct.OwnerGuest != null ? ct.OwnerGuest.GuestCode : "",
                        BookingDate = ct.TourDate,
                        Location = ct.City != null ? ct.City.CityName : "Bilinmiyor",
                        Description = $"Şehir Turu - {ct.DurationHours} saat",
                        Amount = ct.FinalPrice,
                        Status = "Aktif",
                        PersonnelId = ct.PersonnelId,
                        PersonnelName = ct.Personnel != null ? ct.Personnel.FullName : ""
                    })
                    .ToListAsync();

                var weekYachtTours = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Include(yt => yt.Personnel)
                    .Include(yt => yt.City)
                    .Where(yt => yt.TourDate.Date > today && yt.TourDate.Date <= weekEnd)
                    .Select(yt => new UpcomingBookingDto
                    {
                        Id = yt.Id,
                        Type = "YachtTour",
                        GuestName = yt.OwnerGuest != null ? yt.OwnerGuest.FullName : "Bilinmiyor",
                        GuestCode = yt.OwnerGuest != null ? yt.OwnerGuest.GuestCode : "",
                        BookingDate = yt.TourDate,
                        Location = yt.City != null ? yt.City.CityName : "Bilinmiyor",
                        Description = $"Yat Turu - {yt.YachtName}",
                        Amount = yt.FinalPrice,
                        Status = "Aktif",
                        PersonnelId = yt.PersonnelId,
                        PersonnelName = yt.Personnel != null ? yt.Personnel.FullName : ""
                    })
                    .ToListAsync();

                // Bu ayın rezervasyonları
                var monthTransfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Where(t => t.TransferDate.Date > weekEnd && t.TransferDate.Date <= monthEnd)
                    .Select(t => new UpcomingBookingDto
                    {
                        Id = t.Id,
                        Type = "Transfer",
                        GuestName = t.Guest != null ? t.Guest.FullName : "Bilinmiyor",
                        GuestCode = t.Guest != null ? t.Guest.GuestCode : "",
                        BookingDate = t.TransferDate,
                        Location = $"{t.PickupAddress} → {t.DropoffAddress}",
                        Description = t.Note ?? "Transfer",
                        Amount = t.FinalPrice,
                        Status = t.Status.ToString(),
                        PersonnelId = t.PersonnelId,
                        PersonnelName = t.Personnel != null ? t.Personnel.FullName : ""
                    })
                    .ToListAsync();

                var monthCityTours = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Include(ct => ct.Personnel)
                    .Include(ct => ct.City)
                    .Where(ct => ct.TourDate.Date > weekEnd && ct.TourDate.Date <= monthEnd)
                    .Select(ct => new UpcomingBookingDto
                    {
                        Id = ct.Id,
                        Type = "CityTour",
                        GuestName = ct.OwnerGuest != null ? ct.OwnerGuest.FullName : "Bilinmiyor",
                        GuestCode = ct.OwnerGuest != null ? ct.OwnerGuest.GuestCode : "",
                        BookingDate = ct.TourDate,
                        Location = ct.City != null ? ct.City.CityName : "Bilinmiyor",
                        Description = $"Şehir Turu - {ct.DurationHours} saat",
                        Amount = ct.FinalPrice,
                        Status = "Aktif",
                        PersonnelId = ct.PersonnelId,
                        PersonnelName = ct.Personnel != null ? ct.Personnel.FullName : ""
                    })
                    .ToListAsync();

                var monthYachtTours = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Include(yt => yt.Personnel)
                    .Include(yt => yt.City)
                    .Where(yt => yt.TourDate.Date > weekEnd && yt.TourDate.Date <= monthEnd)
                    .Select(yt => new UpcomingBookingDto
                    {
                        Id = yt.Id,
                        Type = "YachtTour",
                        GuestName = yt.OwnerGuest != null ? yt.OwnerGuest.FullName : "Bilinmiyor",
                        GuestCode = yt.OwnerGuest != null ? yt.OwnerGuest.GuestCode : "",
                        BookingDate = yt.TourDate,
                        Location = yt.City != null ? yt.City.CityName : "Bilinmiyor",
                        Description = $"Yat Turu - {yt.YachtName}",
                        Amount = yt.FinalPrice,
                        Status = "Aktif",
                        PersonnelId = yt.PersonnelId,
                        PersonnelName = yt.Personnel != null ? yt.Personnel.FullName : ""
                    })
                    .ToListAsync();

                var todayBookings = todayTransfers
                    .Concat(todayCityTours)
                    .Concat(todayYachtTours)
                    .OrderBy(b => b.BookingDate)
                    .ToList();

                var weekBookings = weekTransfers
                    .Concat(weekCityTours)
                    .Concat(weekYachtTours)
                    .OrderBy(b => b.BookingDate)
                    .ToList();

                var monthBookings = monthTransfers
                    .Concat(monthCityTours)
                    .Concat(monthYachtTours)
                    .OrderBy(b => b.BookingDate)
                    .ToList();

                var totalUpcoming = todayBookings.Count + weekBookings.Count + monthBookings.Count;

                return new UpcomingBookingsDto
                {
                    Today = todayBookings,
                    ThisWeek = weekBookings,
                    ThisMonth = monthBookings,
                    TotalUpcoming = totalUpcoming
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yaklaşan rezervasyonlar getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<GuestStatisticsCardDto> GetGuestStatisticsCardAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var lastMonthStart = monthStart.AddMonths(-1);
                var lastMonthEnd = monthStart.AddDays(-1);
                var thirtyDaysAgo = today.AddDays(-30);

                var totalGuests = await _guestRepository.GetAll().CountAsync();
                
                var activeGuests = await _guestRepository.GetAll()
                    .Where(g => g.Transfers.Any(t => t.TransferDate >= thirtyDaysAgo) ||
                               g.CityTours.Any(ct => ct.TourDate >= thirtyDaysAgo) ||
                               g.YachtTours.Any(yt => yt.TourDate >= thirtyDaysAgo))
                    .CountAsync();

                var specialGuests = await _guestRepository.GetAll()
                    .Where(g => g.IsSpecialGuest)
                    .CountAsync();

                var newGuestsThisMonth = await _guestRepository.GetAll()
                    .Where(g => g.CreatedDate.Date >= monthStart)
                    .CountAsync();

                var newGuestsLastMonth = await _guestRepository.GetAll()
                    .Where(g => g.CreatedDate.Date >= lastMonthStart && g.CreatedDate.Date <= lastMonthEnd)
                    .CountAsync();

                var guestGrowthPercentage = newGuestsLastMonth > 0
                    ? ((newGuestsThisMonth - newGuestsLastMonth) / (decimal)newGuestsLastMonth) * 100
                    : 0;

                var topGuests = await _guestRepository.GetAll()
                    .Select(g => new TopGuestDto
                    {
                        GuestId = g.Id,
                        FullName = g.FullName,
                        GuestCode = g.GuestCode,
                        BookingCount = g.Transfers.Count + g.CityTours.Count + g.YachtTours.Count,
                        TotalSpent = g.Transfers.Sum(t => (decimal?)t.FinalPrice) ?? 0 +
                                     g.CityTours.Sum(ct => (decimal?)ct.FinalPrice) ?? 0 +
                                     g.YachtTours.Sum(yt => (decimal?)yt.FinalPrice) ?? 0
                    })
                    .OrderByDescending(g => g.TotalSpent)
                    .Take(10)
                    .ToListAsync();

                return new GuestStatisticsCardDto
                {
                    TotalGuests = totalGuests,
                    ActiveGuests = activeGuests,
                    SpecialGuests = specialGuests,
                    NewGuestsThisMonth = newGuestsThisMonth,
                    NewGuestsLastMonth = newGuestsLastMonth,
                    GuestGrowthPercentage = guestGrowthPercentage,
                    TopGuests = topGuests
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir istatistik kartı getirilirken hata: {ex.Message}");
                throw;
            }
        }

        private async Task<int> GetBookingCountForDateAsync(DateTime date)
        {
            var cityTourCount = await _cityTourRepository.GetAll()
                .Where(ct => ct.TourDate.Date == date.Date)
                .CountAsync();

            var yachtTourCount = await _yachtTourRepository.GetAll()
                .Where(yt => yt.TourDate.Date == date.Date)
                .CountAsync();

            var transferCount = await _transferRepository.GetAll()
                .Where(t => t.TransferDate.Date == date.Date)
                .CountAsync();

            return cityTourCount + yachtTourCount + transferCount;
        }

        private async Task<int> GetBookingCountForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var cityTourCount = await _cityTourRepository.GetAll()
                .Where(ct => ct.TourDate.Date >= startDate.Date && ct.TourDate.Date <= endDate.Date)
                .CountAsync();

            var yachtTourCount = await _yachtTourRepository.GetAll()
                .Where(yt => yt.TourDate.Date >= startDate.Date && yt.TourDate.Date <= endDate.Date)
                .CountAsync();

            var transferCount = await _transferRepository.GetAll()
                .Where(t => t.TransferDate.Date >= startDate.Date && t.TransferDate.Date <= endDate.Date)
                .CountAsync();

            return cityTourCount + yachtTourCount + transferCount;
        }

        /// <summary>
        /// Ödenmemiş servisler - PaymentEntity'den hesaplanır
        /// Bir servis "unpaid" = toplam ödeme < servis tutarı
        /// </summary>
        public async Task<UnpaidServicesDto> GetUnpaidServicesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.Date.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow.Date.AddDays(7);

            var result = new UnpaidServicesDto();
            var items = new List<UnpaidServiceItemDto>();

            // Get all services in the date range and calculate their payment status using canonical method
            var transfers = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Include(t => t.PickupCity)
                .Where(t => !t.IsDeleted && t.TransferDate >= start && t.TransferDate <= end)
                .ToListAsync();

            foreach (var t in transfers)
            {
                var status = await _paymentStatusService.GetServicePaymentStatusAsync(t.Id, "Transfer");
                if (status.PaymentStatus != "Paid") // Only include unpaid/partially paid
                {
                    items.Add(new UnpaidServiceItemDto
                    {
                        ServiceType = "Transfer",
                        ServiceId = t.Id,
                        ServiceDate = t.TransferDate,
                        GuestName = t.Guest?.FullName ?? "Bilinmiyor",
                        GuestId = t.GuestId,
                        RoomNumber = t.Guest?.RoomNumber,
                        CityName = t.PickupCity?.CityName,
                        ServiceAmount = status.ServiceAmount,
                        PaidAmount = status.PaidAmount,
                        RemainingAmount = status.RemainingAmount,
                        Currency = status.Currency,
                        Status = t.Status,
                        PaymentStatus = status.PaymentStatus,
                        DaysOverdue = (int)(DateTime.UtcNow.Date - t.TransferDate.Date).TotalDays
                    });
                }
            }

            var cityTours = await _cityTourRepository.GetAll()
                .Include(ct => ct.OwnerGuest)
                .Include(ct => ct.City)
                .Where(ct => !ct.IsDeleted && ct.TourDate >= start && ct.TourDate <= end)
                .ToListAsync();

            foreach (var ct in cityTours)
            {
                var status = await _paymentStatusService.GetServicePaymentStatusAsync(ct.Id, "CityTour");
                if (status.PaymentStatus != "Paid") // Only include unpaid/partially paid
                {
                    items.Add(new UnpaidServiceItemDto
                    {
                        ServiceType = "CityTour",
                        ServiceId = ct.Id,
                        ServiceDate = ct.TourDate,
                        GuestName = ct.OwnerGuest?.FullName ?? "Bilinmiyor",
                        GuestId = ct.OwnerGuestId,
                        RoomNumber = ct.OwnerGuest?.RoomNumber,
                        CityName = ct.City?.CityName,
                        ServiceAmount = status.ServiceAmount,
                        PaidAmount = status.PaidAmount,
                        RemainingAmount = status.RemainingAmount,
                        Currency = status.Currency,
                        Status = null,
                        PaymentStatus = status.PaymentStatus,
                        DaysOverdue = (int)(DateTime.UtcNow.Date - ct.TourDate.Date).TotalDays
                    });
                }
            }

            var yachtTours = await _yachtTourRepository.GetAll()
                .Include(yt => yt.OwnerGuest)
                .Include(yt => yt.City)
                .Where(yt => !yt.IsDeleted && yt.TourDate >= start && yt.TourDate <= end)
                .ToListAsync();

            foreach (var yt in yachtTours)
            {
                var status = await _paymentStatusService.GetServicePaymentStatusAsync(yt.Id, "YachtTour");
                if (status.PaymentStatus != "Paid") // Only include unpaid/partially paid
                {
                    items.Add(new UnpaidServiceItemDto
                    {
                        ServiceType = "YachtTour",
                        ServiceId = yt.Id,
                        ServiceDate = yt.TourDate,
                        GuestName = yt.OwnerGuest?.FullName ?? "Bilinmiyor",
                        GuestId = yt.OwnerGuestId,
                        RoomNumber = yt.OwnerGuest?.RoomNumber,
                        CityName = yt.City?.CityName,
                        ServiceAmount = status.ServiceAmount,
                        PaidAmount = status.PaidAmount,
                        RemainingAmount = status.RemainingAmount,
                        Currency = status.Currency,
                        Status = null,
                        PaymentStatus = status.PaymentStatus,
                        DaysOverdue = (int)(DateTime.UtcNow.Date - yt.TourDate.Date).TotalDays
                    });
                }
            }

            result.Items = items.OrderBy(i => i.ServiceDate).ToList();
            
            // Currency bazlı toplam kalan tutar
            result.TotalRemainingByCurrency = items
                .GroupBy(i => i.Currency ?? "TRY")
                .ToDictionary(g => g.Key, g => g.Sum(i => i.RemainingAmount));
            
            result.TotalUnpaidCount = items.Count(i => i.PaymentStatus == "Unpaid");
            result.PartiallyPaidCount = items.Count(i => i.PaymentStatus == "PartiallyPaid");

            return result;
        }

        public async Task<UpcomingServicesDto> GetUpcomingServicesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var now = DateTime.UtcNow;
            var start = startDate ?? now;
            var end = endDate ?? now.AddDays(3);

            var result = new UpcomingServicesDto();

            var transfers = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Include(t => t.PickupCity)
                .Where(t => !t.IsDeleted && t.TransferDate >= start && t.TransferDate <= end)
                .Select(t => new UpcomingServiceItemDto
                {
                    ServiceType = "Transfer",
                    ServiceId = t.Id,
                    ServiceDate = t.TransferDate,
                    GuestName = t.Guest.FullName,
                    RoomNumber = t.Guest.RoomNumber,
                    CityName = t.PickupCity != null ? t.PickupCity.CityName : null,
                    Status = t.Status,
                    IsUrgent = t.TransferDate <= now.AddHours(3)
                })
                .ToListAsync();

            var cityTours = await _cityTourRepository.GetAll()
                .Include(ct => ct.OwnerGuest)
                .Include(ct => ct.City)
                .Where(ct => !ct.IsDeleted && ct.TourDate >= start && ct.TourDate <= end)
                .Select(ct => new UpcomingServiceItemDto
                {
                    ServiceType = "CityTour",
                    ServiceId = ct.Id,
                    ServiceDate = ct.TourDate,
                    GuestName = ct.OwnerGuest.FullName,
                    RoomNumber = ct.OwnerGuest.RoomNumber,
                    CityName = ct.City.CityName,
                    Status = null,
                    IsUrgent = ct.TourDate <= now.AddDays(1)
                })
                .ToListAsync();

            var yachtTours = await _yachtTourRepository.GetAll()
                .Include(yt => yt.OwnerGuest)
                .Include(yt => yt.City)
                .Where(yt => !yt.IsDeleted && yt.TourDate >= start && yt.TourDate <= end)
                .Select(yt => new UpcomingServiceItemDto
                {
                    ServiceType = "YachtTour",
                    ServiceId = yt.Id,
                    ServiceDate = yt.TourDate,
                    GuestName = yt.OwnerGuest.FullName,
                    RoomNumber = yt.OwnerGuest.RoomNumber,
                    CityName = yt.City.CityName,
                    Status = null,
                    IsUrgent = yt.TourDate <= now.AddDays(1)
                })
                .ToListAsync();

            result.Items = transfers
                .Concat(cityTours)
                .Concat(yachtTours)
                .OrderBy(i => i.ServiceDate)
                .ToList();

            return result;
        }

        public async Task<CriticalEventsDto> GetCriticalEventsAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var result = new CriticalEventsDto();

            // 1. Services starting within next 2 hours
            var next2Hours = now.AddHours(2);
            var urgentServices = new List<CriticalEventItemDto>();

            // Transfers starting soon
            var urgentTransfers = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Where(t => !t.IsDeleted &&
                           t.TransferDate >= now &&
                           t.TransferDate <= next2Hours &&
                           t.Status != "Completed" &&
                           t.Status != "Cancelled")
                .Select(t => new CriticalEventItemDto
                {
                    Type = "Transfer",
                    Id = t.Id,
                    Title = $"Transfer: {t.Guest.FullName}",
                    Description = $"{t.PickupAddress} → {t.DropoffAddress}",
                    Time = t.TransferDate,
                    Urgency = "HIGH",
                    ActionRequired = "Konuk hazır mı kontrol et"
                })
                .ToListAsync();

            urgentServices.AddRange(urgentTransfers);

            // City Tours starting soon
            var urgentCityTours = await _cityTourRepository.GetAll()
                .Include(ct => ct.OwnerGuest)
                .Where(ct => !ct.IsDeleted &&
                            ct.TourDate >= now &&
                            ct.TourDate <= next2Hours)
                .Select(ct => new CriticalEventItemDto
                {
                    Type = "CityTour",
                    Id = ct.Id,
                    Title = $"Şehir Turu: {ct.OwnerGuest.FullName}",
                    Description = $"{ct.DurationHours} saat, {(ct.AdultCount ?? 1) + (ct.ChildCount ?? 0) + (ct.InfantCount ?? 0)} kişi",
                    Time = ct.TourDate,
                    Urgency = "HIGH",
                    ActionRequired = "Rehber ve araç hazır mı?"
                })
                .ToListAsync();

            urgentServices.AddRange(urgentCityTours);

            // Yacht Tours starting soon
            var urgentYachtTours = await _yachtTourRepository.GetAll()
                .Include(yt => yt.OwnerGuest)
                .Where(yt => !yt.IsDeleted &&
                            yt.TourDate >= now &&
                            yt.TourDate <= next2Hours)
                .Select(yt => new CriticalEventItemDto
                {
                    Type = "YachtTour",
                    Id = yt.Id,
                    Title = $"Yat Turu: {yt.OwnerGuest.FullName}",
                    Description = $"{yt.NumberOfPeople} kişi, {yt.YachtName}",
                    Time = yt.TourDate,
                    Urgency = "CRITICAL",
                    ActionRequired = "Güvenlik brifingi ve can yelekleri hazır mı?"
                })
                .ToListAsync();

            urgentServices.AddRange(urgentYachtTours);

            result.UrgentServices = urgentServices.OrderBy(u => u.Time).ToList();

            // 2. Arrivals requiring transport coordination
            var arrivalsNeedingTransport = await _guestRepository.GetAll()
                .Include(g => g.RoomAssignments.Where(ra => ra.StartDate <= today && (ra.EndDate == null || ra.EndDate >= today)))
                .Where(g => !g.IsDeleted &&
                           g.CheckInDate == today &&
                           g.RoomAssignments.Any(ra => ra.StartDate <= today && (ra.EndDate == null || ra.EndDate >= today)))
                .Select(g => new CriticalEventItemDto
                {
                    Type = "Arrival",
                    Id = g.Id,
                    Title = $"Varış: {g.FullName}",
                    Description = $"Oda: {(g.RoomAssignments.Any() ? g.RoomAssignments.First().RoomNumber : "Bilinmiyor")}",
                    Time = today.AddHours(14), // Assume 2 PM arrival
                    Urgency = "MEDIUM",
                    ActionRequired = "Transfer düzenlendi mi?"
                })
                .ToListAsync();

            result.ArrivalsNeedingTransport = arrivalsNeedingTransport;

            // 3. Departures requiring checkout coordination
            var departuresToday = await _guestRepository.GetAll()
                .Include(g => g.RoomAssignments.Where(ra => ra.StartDate <= today && (ra.EndDate == null || ra.EndDate >= today)))
                .Where(g => !g.IsDeleted &&
                           g.CheckOutDate == today &&
                           g.RoomAssignments.Any(ra => ra.StartDate <= today && (ra.EndDate == null || ra.EndDate >= today)))
                .Select(g => new CriticalEventItemDto
                {
                    Type = "Departure",
                    Id = g.Id,
                    Title = $"Ayrılış: {g.FullName}",
                    Description = $"Oda: {(g.RoomAssignments.Any() ? g.RoomAssignments.First().RoomNumber : "Bilinmiyor")}",
                    Time = today.AddHours(11), // Assume 11 AM checkout
                    Urgency = "MEDIUM",
                    ActionRequired = "Checkout tamamlandı mı?"
                })
                .ToListAsync();

            result.DeparturesRequiringCheckout = departuresToday;

            // 4. Unconfirmed services for tomorrow
            var tomorrow = today.AddDays(1);
            var unconfirmedServices = new List<CriticalEventItemDto>();

            // Unconfirmed transfers for tomorrow
            var unconfirmedTransfers = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Where(t => !t.IsDeleted &&
                           t.TransferDate >= tomorrow &&
                           t.TransferDate < tomorrow.AddDays(1) &&
                           (t.Status == null || t.Status == "Pending"))
                .Select(t => new CriticalEventItemDto
                {
                    Type = "UnconfirmedTransfer",
                    Id = t.Id,
                    Title = $"Onaylanmamış Transfer: {t.Guest.FullName}",
                    Description = $"{t.PickupAddress} → {t.DropoffAddress}",
                    Time = t.TransferDate,
                    Urgency = "MEDIUM",
                    ActionRequired = "Konukla onay al"
                })
                .ToListAsync();

            unconfirmedServices.AddRange(unconfirmedTransfers);

            // Unconfirmed tours for tomorrow
            var unconfirmedCityTours = await _cityTourRepository.GetAll()
                .Include(ct => ct.OwnerGuest)
                .Where(ct => !ct.IsDeleted &&
                            ct.TourDate >= tomorrow &&
                            ct.TourDate < tomorrow.AddDays(1))
                // Assuming we add a Confirmed field later
                .Select(ct => new CriticalEventItemDto
                {
                    Type = "UnconfirmedCityTour",
                    Id = ct.Id,
                    Title = $"Tur Onayı: {ct.OwnerGuest.FullName}",
                    Description = $"{ct.DurationHours} saat şehir turu",
                    Time = ct.TourDate,
                    Urgency = "LOW",
                    ActionRequired = "Tur detaylarını doğrula"
                })
                .ToListAsync();

            unconfirmedServices.AddRange(unconfirmedCityTours);

            result.UnconfirmedServices = unconfirmedServices.OrderBy(u => u.Time).ToList();

            return result;
        }
    }
}

