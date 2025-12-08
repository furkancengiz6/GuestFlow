using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Dashboard
{
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
                var totalGuests = await _guestRepository.GetAll().CountAsync();
                var activeGuests = await _guestRepository.GetAll()
                    .Where(g => g.Transfers.Any(t => t.TransferDate >= thirtyDaysAgo) ||
                               g.CityTours.Any(ct => ct.TourDate >= thirtyDaysAgo) ||
                               g.YachtTours.Any(yt => yt.TourDate >= thirtyDaysAgo))
                    .CountAsync();

                var totalPersonnel = await _personnelRepository.GetAll().CountAsync();
                var totalTransfers = await _transferRepository.GetAll().CountAsync();
                var totalCityTours = await _cityTourRepository.GetAll().CountAsync();
                var totalYachtTours = await _yachtTourRepository.GetAll().CountAsync();
                var totalInvoices = await _invoiceRepository.GetAll().CountAsync();

                var totalRevenue = await _cityTourRepository.GetAll()
                    .SumAsync(ct => (decimal?)ct.FinalPrice) ?? 0 +
                    await _yachtTourRepository.GetAll()
                    .SumAsync(yt => (decimal?)yt.FinalPrice) ?? 0 +
                    await _transferRepository.GetAll()
                    .SumAsync(t => (decimal?)t.FinalPrice) ?? 0;

                return new QuickStatsDto
                {
                    TotalGuests = totalGuests,
                    ActiveGuests = activeGuests,
                    TotalPersonnel = totalPersonnel,
                    TotalTransfers = totalTransfers,
                    TotalCityTours = totalCityTours,
                    TotalYachtTours = totalYachtTours,
                    TotalInvoices = totalInvoices,
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

        private async Task<decimal> GetRevenueForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var cityTourRevenue = await _cityTourRepository.GetAll()
                .Where(ct => ct.TourDate.Date >= startDate.Date && ct.TourDate.Date <= endDate.Date)
                .SumAsync(ct => (decimal?)ct.FinalPrice) ?? 0;

            var yachtTourRevenue = await _yachtTourRepository.GetAll()
                .Where(yt => yt.TourDate.Date >= startDate.Date && yt.TourDate.Date <= endDate.Date)
                .SumAsync(yt => (decimal?)yt.FinalPrice) ?? 0;

            var transferRevenue = await _transferRepository.GetAll()
                .Where(t => t.TransferDate.Date >= startDate.Date && t.TransferDate.Date <= endDate.Date)
                .SumAsync(t => (decimal?)t.FinalPrice) ?? 0;

            return cityTourRevenue + yachtTourRevenue + transferRevenue;
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
            var transferStats = await _transferRepository.GetAll()
                .GroupBy(t => "Transfer")
                .Select(g => new PopularServiceDto
                {
                    ServiceType = "Transfer",
                    BookingCount = g.Count(),
                    TotalRevenue = g.Sum(t => t.FinalPrice),
                    AveragePrice = g.Average(t => t.FinalPrice)
                })
                .FirstOrDefaultAsync();

            var cityTourStats = await _cityTourRepository.GetAll()
                .GroupBy(ct => "CityTour")
                .Select(g => new PopularServiceDto
                {
                    ServiceType = "CityTour",
                    BookingCount = g.Count(),
                    TotalRevenue = g.Sum(ct => ct.FinalPrice),
                    AveragePrice = g.Average(ct => ct.FinalPrice)
                })
                .FirstOrDefaultAsync();

            var yachtTourStats = await _yachtTourRepository.GetAll()
                .GroupBy(yt => "YachtTour")
                .Select(g => new PopularServiceDto
                {
                    ServiceType = "YachtTour",
                    BookingCount = g.Count(),
                    TotalRevenue = g.Sum(yt => yt.FinalPrice),
                    AveragePrice = g.Average(yt => yt.FinalPrice)
                })
                .FirstOrDefaultAsync();

            var services = new List<PopularServiceDto>();
            if (transferStats != null) services.Add(transferStats);
            if (cityTourStats != null) services.Add(cityTourStats);
            if (yachtTourStats != null) services.Add(yachtTourStats);

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
    }
}

