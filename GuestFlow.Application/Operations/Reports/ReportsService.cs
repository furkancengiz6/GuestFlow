using GuestFlow.Application.Models.AI;
using GuestFlow.Application.Operations.AI;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Reports
{
    /// <summary>
    /// Raporlama servisi - Tahsilat bazlı gelir hesaplaması (PaymentEntity'den)
    /// </summary>
    public class ReportsService : IReportsService
    {
        private readonly IRepository<DailyRevenueEntity> _dailyRevenueRepository;
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<InvoiceItemEntity> _invoiceItemRepository;
        private readonly IRepository<JournalEntry> _journalEntryRepository;
        private readonly IRepository<JournalLine> _journalLineRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<GuestReview> _reviewRepository;
        private readonly GuestFlow.Application.Operations.Invoice.IPdfService _pdfService;
        private readonly IAIAssistantService _aiAssistantService;
        private readonly ILogger<ReportsService> _logger;

        public ReportsService(
            IRepository<DailyRevenueEntity> dailyRevenueRepository,
            IRepository<PaymentEntity> paymentRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<InvoiceItemEntity> invoiceItemRepository,
            IRepository<JournalEntry> journalEntryRepository,
            IRepository<JournalLine> journalLineRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<GuestReview> reviewRepository,
            GuestFlow.Application.Operations.Invoice.IPdfService pdfService,
            IAIAssistantService aiAssistantService,
            ILogger<ReportsService> logger)
        {
            _dailyRevenueRepository = dailyRevenueRepository;
            _paymentRepository = paymentRepository;
            _guestRepository = guestRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _transferRepository = transferRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _journalEntryRepository = journalEntryRepository;
            _journalLineRepository = journalLineRepository;
            _cityRepository = cityRepository;
            _personnelRepository = personnelRepository;
            _reviewRepository = reviewRepository;
            _pdfService = pdfService;
            _aiAssistantService = aiAssistantService;
            _logger = logger;
        }

        /// <summary>
        /// Gelir özeti - Tahsilat bazlı (PaymentEntity'den hesaplanır)
        /// Gelir = Tamamlanmış ödemeler (Status = Completed)
        /// </summary>
        public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime? startDate = null, DateTime? endDate = null, string? serviceType = null, int? personnelId = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                // Tamamlanmış ödemeleri çek (tahsilat bazlı gelir)
                var completedPaymentsQuery = _paymentRepository.GetAll()
                    .Where(p => p.PaymentDate.Date >= start.Date && 
                               p.PaymentDate.Date <= end.Date && 
                               p.Status == PaymentStatus.Completed && 
                               !p.IsDeleted);

                // Servis tipi filtresi
                if (!string.IsNullOrEmpty(serviceType))
                {
                    if (serviceType == "Transfer")
                        completedPaymentsQuery = completedPaymentsQuery.Where(p => p.TransferId.HasValue);
                    else if (serviceType == "CityTour")
                        completedPaymentsQuery = completedPaymentsQuery.Where(p => p.CityTourId.HasValue);
                    else if (serviceType == "YachtTour")
                        completedPaymentsQuery = completedPaymentsQuery.Where(p => p.YachtTourId.HasValue);
                }

                // Personel filtresi (CollectedByPersonnelId)
                if (personnelId.HasValue)
                {
                    completedPaymentsQuery = completedPaymentsQuery.Where(p => p.CollectedByPersonnelId == personnelId.Value);
                }

                var completedPayments = await completedPaymentsQuery.ToListAsync();

                // İade edilen ödemeleri çek
                var refundedPaymentsQuery = _paymentRepository.GetAll()
                    .Where(p => p.RefundDate.HasValue &&
                               p.RefundDate.Value.Date >= start.Date && 
                               p.RefundDate.Value.Date <= end.Date && 
                               p.Status == PaymentStatus.Refunded && 
                               !p.IsDeleted);

                // Servis tipi filtresi
                if (!string.IsNullOrEmpty(serviceType))
                {
                    if (serviceType == "Transfer")
                        refundedPaymentsQuery = refundedPaymentsQuery.Where(p => p.TransferId.HasValue);
                    else if (serviceType == "CityTour")
                        refundedPaymentsQuery = refundedPaymentsQuery.Where(p => p.CityTourId.HasValue);
                    else if (serviceType == "YachtTour")
                        refundedPaymentsQuery = refundedPaymentsQuery.Where(p => p.YachtTourId.HasValue);
                }

                // Personel filtresi
                if (personnelId.HasValue)
                {
                    refundedPaymentsQuery = refundedPaymentsQuery.Where(p => p.CollectedByPersonnelId == personnelId.Value);
                }

                var refundedPayments = await refundedPaymentsQuery.ToListAsync();

                // Currency bazlı grupla
                var currencies = completedPayments.Select(p => p.Currency)
                    .Union(refundedPayments.Select(p => p.Currency))
                    .Distinct()
                    .ToList();

                var result = new RevenueSummaryDto
                {
                    StartDate = start,
                    EndDate = end,
                    TotalRevenueByCurrency = new Dictionary<string, decimal>(),
                    TransferRevenueByCurrency = new Dictionary<string, decimal>(),
                    CityTourRevenueByCurrency = new Dictionary<string, decimal>(),
                    YachtTourRevenueByCurrency = new Dictionary<string, decimal>(),
                    GeneralRevenueByCurrency = new Dictionary<string, decimal>(),
                    RefundedAmountByCurrency = new Dictionary<string, decimal>(),
                    NetRevenueByCurrency = new Dictionary<string, decimal>()
                };

                foreach (var currency in currencies)
                {
                    var currencyPayments = completedPayments.Where(p => p.Currency == currency).ToList();
                    var currencyRefunds = refundedPayments.Where(p => p.Currency == currency).ToList();

                    var transferRevenue = currencyPayments.Where(p => p.TransferId.HasValue).Sum(p => p.Amount);
                    var cityTourRevenue = currencyPayments.Where(p => p.CityTourId.HasValue).Sum(p => p.Amount);
                    var yachtTourRevenue = currencyPayments.Where(p => p.YachtTourId.HasValue).Sum(p => p.Amount);
                    var generalRevenue = currencyPayments
                        .Where(p => !p.TransferId.HasValue && !p.CityTourId.HasValue && !p.YachtTourId.HasValue)
                        .Sum(p => p.Amount);

                    var totalRevenue = transferRevenue + cityTourRevenue + yachtTourRevenue + generalRevenue;
                    var refundedAmount = currencyRefunds.Sum(p => p.Amount);
                    var netRevenue = totalRevenue - refundedAmount;

                    result.TotalRevenueByCurrency[currency] = totalRevenue;
                    result.TransferRevenueByCurrency[currency] = transferRevenue;
                    result.CityTourRevenueByCurrency[currency] = cityTourRevenue;
                    result.YachtTourRevenueByCurrency[currency] = yachtTourRevenue;
                    result.GeneralRevenueByCurrency[currency] = generalRevenue;
                    result.RefundedAmountByCurrency[currency] = refundedAmount;
                    result.NetRevenueByCurrency[currency] = netRevenue;
                }

                // Rezervasyon sayıları (servis bazlı)
                var cityTourQuery = _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date >= start.Date && ct.TourDate.Date <= end.Date);
                
                var yachtTourQuery = _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date >= start.Date && yt.TourDate.Date <= end.Date);
                
                var transferQuery = _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date >= start.Date && t.TransferDate.Date <= end.Date);

                // Personel filtresi (servis bazlı)
                if (personnelId.HasValue)
                {
                    cityTourQuery = cityTourQuery.Where(ct => ct.PersonnelId == personnelId.Value);
                    yachtTourQuery = yachtTourQuery.Where(yt => yt.PersonnelId == personnelId.Value);
                    transferQuery = transferQuery.Where(t => t.PersonnelId == personnelId.Value);
                }

                // Servis tipi filtresi
                if (!string.IsNullOrEmpty(serviceType))
                {
                    if (serviceType == "CityTour")
                    {
                        var cityTourCount = await cityTourQuery.CountAsync();
                        result.CityTourCount = cityTourCount;
                        result.YachtTourCount = 0;
                        result.TransferCount = 0;
                        result.TotalBookings = cityTourCount;
                    }
                    else if (serviceType == "YachtTour")
                    {
                        var yachtTourCount = await yachtTourQuery.CountAsync();
                        result.CityTourCount = 0;
                        result.YachtTourCount = yachtTourCount;
                        result.TransferCount = 0;
                        result.TotalBookings = yachtTourCount;
                    }
                    else if (serviceType == "Transfer")
                    {
                        var transferCount = await transferQuery.CountAsync();
                        result.CityTourCount = 0;
                        result.YachtTourCount = 0;
                        result.TransferCount = transferCount;
                        result.TotalBookings = transferCount;
                    }
                    else
                    {
                        result.CityTourCount = 0;
                        result.YachtTourCount = 0;
                        result.TransferCount = 0;
                        result.TotalBookings = 0;
                    }
                }
                else
                {
                    var cityTourCount = await cityTourQuery.CountAsync();
                    var yachtTourCount = await yachtTourQuery.CountAsync();
                    var transferCount = await transferQuery.CountAsync();

                    result.CityTourCount = cityTourCount;
                    result.YachtTourCount = yachtTourCount;
                    result.TransferCount = transferCount;
                    result.TotalBookings = cityTourCount + yachtTourCount + transferCount;
                    result.TotalPaymentCount = completedPayments.Count;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Gelir özeti getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<GuestStatisticsDto> GetGuestStatisticsAsync()
        {
            try
            {
                var totalGuests = await _guestRepository.GetAll().CountAsync();
                var specialGuests = await _guestRepository.GetAll(x => x.IsSpecialGuest).CountAsync();
                var regularGuests = totalGuests - specialGuests;

                // Son 30 günde rezervasyon yapan misafirler aktif sayılır
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var activeGuests = await _guestRepository.GetAll()
                    .Where(g => g.Transfers.Any(t => t.TransferDate >= thirtyDaysAgo) ||
                               g.CityTours.Any(ct => ct.TourDate >= thirtyDaysAgo) ||
                               g.YachtTours.Any(yt => yt.TourDate >= thirtyDaysAgo))
                    .CountAsync();

                // En çok rezervasyon yapan misafirler
                var topGuests = await _guestRepository.GetAll()
                    .Select(g => new TopGuestDto
                    {
                        GuestId = g.Id,
                        FullName = g.FullName,
                        GuestCode = g.GuestCode,
                        BookingCount = g.Transfers.Count + g.CityTours.Count + g.YachtTours.Count,
                        TotalSpent = g.Transfers.Sum(t => t.FinalPrice) +
                                     g.CityTours.Sum(ct => ct.FinalPrice) +
                                     g.YachtTours.Sum(yt => yt.FinalPrice)
                    })
                    .OrderByDescending(g => g.TotalSpent)
                    .Take(10)
                    .ToListAsync();

                // Uyruğa göre dağılım
                var guestsByNationality = await _guestRepository.GetAll()
                    .GroupBy(g => g.Nationality)
                    .Select(g => new { Nationality = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Nationality, x => x.Count);

                return new GuestStatisticsDto
                {
                    TotalGuests = totalGuests,
                    ActiveGuests = activeGuests,
                    SpecialGuests = specialGuests,
                    RegularGuests = regularGuests,
                    TopGuests = topGuests,
                    GuestsByNationality = guestsByNationality
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir istatistikleri getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<TourStatisticsDto> GetTourStatisticsAsync(string? tourType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;
                var now = DateTime.UtcNow;

                if (tourType == "CityTour" || string.IsNullOrEmpty(tourType))
                {
                    var cityTours = _cityTourRepository.GetAll()
                        .Where(ct => ct.TourDate.Date >= start.Date && ct.TourDate.Date <= end.Date);

                    var totalTours = await cityTours.CountAsync();
                    // REVENUE REALITY: Revenue = collected money only (from PaymentEntity)
                    var cityTourIds = await cityTours.Select(ct => ct.Id).ToListAsync();
                    var totalRevenue = await _paymentRepository.GetAll()
                        .Where(p => p.CityTourId.HasValue && cityTourIds.Contains(p.CityTourId.Value) && p.Status == PaymentStatus.Completed)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0;
                    var averageBookedPrice = await cityTours.AverageAsync(ct => (decimal?)ct.FinalPrice) ?? 0;
                    var averagePrice = averageBookedPrice;
                    var completedTours = await cityTours.Where(ct => ct.TourDate < now).CountAsync();
                    var upcomingTours = await cityTours.Where(ct => ct.TourDate >= now).CountAsync();

                    var toursByLanguage = await cityTours
                        .GroupBy(ct => ct.Language)
                        .Select(g => new { Language = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.Language, x => x.Count);

                    var toursByCity = await cityTours
                        .Include(ct => ct.City)
                        .GroupBy(ct => ct.City != null ? ct.City.CityName : "Bilinmiyor")
                        .Select(g => new { City = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.City, x => x.Count);

                    return new TourStatisticsDto
                    {
                        TourType = "CityTour",
                        StartDate = start,
                        EndDate = end,
                        TotalTours = totalTours,
                        TotalRevenue = totalRevenue,
                        AveragePrice = averagePrice,
                        CompletedTours = completedTours,
                        UpcomingTours = upcomingTours,
                        ToursByLanguage = toursByLanguage,
                        ToursByCity = toursByCity
                    };
                }
                else if (tourType == "YachtTour")
                {
                    var yachtTours = _yachtTourRepository.GetAll()
                        .Where(yt => yt.TourDate.Date >= start.Date && yt.TourDate.Date <= end.Date);

                    var totalTours = await yachtTours.CountAsync();
                    // REVENUE REALITY: Revenue = collected money only (from PaymentEntity)
                    var yachtTourIds = await yachtTours.Select(yt => yt.Id).ToListAsync();
                    var totalRevenue = await _paymentRepository.GetAll()
                        .Where(p => p.YachtTourId.HasValue && yachtTourIds.Contains(p.YachtTourId.Value) && p.Status == PaymentStatus.Completed)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0;
                    var averageBookedPrice = await yachtTours.AverageAsync(yt => (decimal?)yt.FinalPrice) ?? 0;
                    var averagePrice = averageBookedPrice;
                    var completedTours = await yachtTours.Where(yt => yt.TourDate < now).CountAsync();
                    var upcomingTours = await yachtTours.Where(yt => yt.TourDate >= now).CountAsync();

                    var toursByCity = await yachtTours
                        .Include(yt => yt.City)
                        .GroupBy(yt => yt.City != null ? yt.City.CityName : "Bilinmiyor")
                        .Select(g => new { City = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.City, x => x.Count);

                    return new TourStatisticsDto
                    {
                        TourType = "YachtTour",
                        StartDate = start,
                        EndDate = end,
                        TotalTours = totalTours,
                        TotalRevenue = totalRevenue,
                        AveragePrice = averagePrice,
                        CompletedTours = completedTours,
                        UpcomingTours = upcomingTours,
                        ToursByLanguage = new Dictionary<string, int>(),
                        ToursByCity = toursByCity
                    };
                }

                return new TourStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tur istatistikleri getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<TransferStatisticsDto> GetTransferStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, int? personnelId = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;
                var now = DateTime.UtcNow;

                var transfers = _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date >= start.Date && t.TransferDate.Date <= end.Date);

                // Personel filtresi
                if (personnelId.HasValue)
                {
                    transfers = transfers.Where(t => t.PersonnelId == personnelId.Value);
                }

                var totalTransfers = await transfers.CountAsync();
                // REVENUE REALITY: Revenue = collected money only (from PaymentEntity)
                var transferIds = await transfers.Select(t => t.Id).ToListAsync();
                var totalRevenue = await _paymentRepository.GetAll()
                    .Where(p => p.TransferId.HasValue && transferIds.Contains(p.TransferId.Value) && p.Status == PaymentStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;
                var averageBookedPrice = await transfers.AverageAsync(t => (decimal?)t.FinalPrice) ?? 0;
                var averagePrice = averageBookedPrice;
                var fromAirportCount = await transfers.Where(t => t.IsFromAirport).CountAsync();
                var toAirportCount = await transfers.Where(t => !t.IsFromAirport).CountAsync();
                var completedTransfers = await transfers.Where(t => t.TransferDate < now).CountAsync();
                var pendingTransfers = await transfers.Where(t => t.TransferDate >= now).CountAsync();

                var transfersByStatus = await transfers
                    .GroupBy(t => t.Status.ToString())
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);

                return new TransferStatisticsDto
                {
                    StartDate = start,
                    EndDate = end,
                    TotalTransfers = totalTransfers,
                    TotalRevenue = totalRevenue,
                    AveragePrice = averagePrice,
                    FromAirportCount = fromAirportCount,
                    ToAirportCount = toAirportCount,
                    CompletedTransfers = completedTransfers,
                    PendingTransfers = pendingTransfers,
                    TransfersByStatus = transfersByStatus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer istatistikleri getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int? year = null)
        {
            try
            {
                var targetYear = year ?? DateTime.UtcNow.Year;
                var startDate = new DateTime(targetYear, 1, 1);
                var endDate = new DateTime(targetYear, 12, 31, 23, 59, 59);

                var monthlyData = new List<MonthlyRevenueDto>();
                var monthNames = new[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

                for (int month = 1; month <= 12; month++)
                {
                    var monthStart = new DateTime(targetYear, month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    // REVENUE REALITY: Revenue = collected money, grouped by PaymentDate
                    var totalRevenue = await _paymentRepository.GetAll()
                        .Where(p => p.PaymentDate.Date >= monthStart.Date && p.PaymentDate.Date <= monthEnd.Date && p.Status == PaymentStatus.Completed)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0;

                    // Booking count remains based on service date (operational)
                    var bookingCount = await _cityTourRepository.GetAll()
                        .Where(ct => ct.TourDate.Date >= monthStart.Date && ct.TourDate.Date <= monthEnd.Date).CountAsync() +
                        await _yachtTourRepository.GetAll()
                        .Where(yt => yt.TourDate.Date >= monthStart.Date && yt.TourDate.Date <= monthEnd.Date).CountAsync() +
                        await _transferRepository.GetAll()
                        .Where(t => t.TransferDate.Date >= monthStart.Date && t.TransferDate.Date <= monthEnd.Date).CountAsync();

                    monthlyData.Add(new MonthlyRevenueDto
                    {
                        Year = targetYear,
                        Month = month,
                        MonthName = monthNames[month - 1],
                        TotalRevenue = totalRevenue,
                        BookingCount = bookingCount
                    });
                }

                return monthlyData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Aylık gelir getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PopularDestinationDto>> GetPopularDestinationsAsync(int? limit = 10)
        {
            try
            {
                var limitValue = limit ?? 10;

                // Şehir turlarından popüler destinasyonlar
                var cityTourDestinations = await _cityTourRepository.GetAll()
                    .Include(ct => ct.City)
                    .Where(ct => ct.City != null)
                    .GroupBy(ct => new { ct.CityId, CityName = ct.City!.CityName, Country = ct.City!.Country })
                    .Select(g => new PopularDestinationDto
                    {
                        CityId = g.Key.CityId,
                        CityName = g.Key.CityName,
                        Country = g.Key.Country,
                        BookingCount = g.Count(),
                        TotalRevenue = g.Sum(ct => ct.FinalPrice)
                    })
                    .ToListAsync();

                // Yat turlarından popüler destinasyonlar
                var yachtTourDestinations = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.City)
                    .Where(yt => yt.City != null)
                    .GroupBy(yt => new { yt.CityId, CityName = yt.City!.CityName, Country = yt.City!.Country })
                    .Select(g => new PopularDestinationDto
                    {
                        CityId = g.Key.CityId,
                        CityName = g.Key.CityName,
                        Country = g.Key.Country,
                        BookingCount = g.Count(),
                        TotalRevenue = g.Sum(yt => yt.FinalPrice)
                    })
                    .ToListAsync();

                // Birleştir ve topla
                var combined = cityTourDestinations
                    .Concat(yachtTourDestinations)
                    .GroupBy(d => d.CityId)
                    .Select(g => new PopularDestinationDto
                    {
                        CityId = g.Key,
                        CityName = g.First().CityName,
                        Country = g.First().Country,
                        BookingCount = g.Sum(d => d.BookingCount),
                        TotalRevenue = g.Sum(d => d.TotalRevenue)
                    })
                    .OrderByDescending(d => d.BookingCount)
                    .Take(limitValue)
                    .ToList();

                return combined;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Popüler destinasyonlar getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var totalGuests = await _guestRepository.GetAll().CountAsync();
                var totalPersonnel = await _personnelRepository.GetAll().CountAsync();

                // Bugünkü gelir
                var todayCityTour = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date == today)
                    .SumAsync(ct => (decimal?)ct.FinalPrice) ?? 0;
                var todayYachtTour = await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date == today)
                    .SumAsync(yt => (decimal?)yt.FinalPrice) ?? 0;
                var todayTransfer = await _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date == today)
                    .SumAsync(t => (decimal?)t.FinalPrice) ?? 0;
                var todayRevenue = todayCityTour + todayYachtTour + todayTransfer;

                // Bu haftanın geliri
                var weekCityTour = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date >= weekStart && ct.TourDate.Date <= today)
                    .SumAsync(ct => (decimal?)ct.FinalPrice) ?? 0;
                var weekYachtTour = await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date >= weekStart && yt.TourDate.Date <= today)
                    .SumAsync(yt => (decimal?)yt.FinalPrice) ?? 0;
                var weekTransfer = await _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date >= weekStart && t.TransferDate.Date <= today)
                    .SumAsync(t => (decimal?)t.FinalPrice) ?? 0;
                var thisWeekRevenue = weekCityTour + weekYachtTour + weekTransfer;

                // Bu ayın geliri
                var monthCityTour = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date >= monthStart && ct.TourDate.Date <= today)
                    .SumAsync(ct => (decimal?)ct.FinalPrice) ?? 0;
                var monthYachtTour = await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date >= monthStart && yt.TourDate.Date <= today)
                    .SumAsync(yt => (decimal?)yt.FinalPrice) ?? 0;
                var monthTransfer = await _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date >= monthStart && t.TransferDate.Date <= today)
                    .SumAsync(t => (decimal?)t.FinalPrice) ?? 0;
                var thisMonthRevenue = monthCityTour + monthYachtTour + monthTransfer;

                // Aktif transferler (bugünden sonraki)
                var activeTransfers = await _transferRepository.GetAll()
                    .Where(t => t.TransferDate >= today).CountAsync();

                // Yaklaşan turlar (bugünden sonraki)
                var upcomingTours = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate >= today).CountAsync() +
                    await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate >= today).CountAsync();

                // Bekleyen faturalar (PdfUrl boş olanlar)
                var pendingInvoices = await _invoiceRepository.GetAll()
                    .Where(i => string.IsNullOrEmpty(i.PdfUrl)).CountAsync();

                // Bugünkü rezervasyonlar
                var todayBookings = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date == today).CountAsync() +
                    await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date == today).CountAsync() +
                    await _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date == today).CountAsync();

                // Son rezervasyonlar
                var recentCityTours = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .OrderByDescending(ct => ct.CreatedDate)
                    .Take(5)
                    .Select(ct => new RecentBookingDto
                    {
                        Id = ct.Id,
                        Type = "CityTour",
                        GuestName = ct.OwnerGuest != null ? ct.OwnerGuest.FullName : "Bilinmiyor",
                        BookingDate = ct.TourDate,
                        Amount = ct.FinalPrice,
                        Status = "Aktif"
                    })
                    .ToListAsync();

                var recentYachtTours = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .OrderByDescending(yt => yt.CreatedDate)
                    .Take(5)
                    .Select(yt => new RecentBookingDto
                    {
                        Id = yt.Id,
                        Type = "YachtTour",
                        GuestName = yt.OwnerGuest != null ? yt.OwnerGuest.FullName : "Bilinmiyor",
                        BookingDate = yt.TourDate,
                        Amount = yt.FinalPrice,
                        Status = "Aktif"
                    })
                    .ToListAsync();

                var recentTransfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .OrderByDescending(t => t.CreatedDate)
                    .Take(5)
                    .Select(t => new RecentBookingDto
                    {
                        Id = t.Id,
                        Type = "Transfer",
                        GuestName = t.Guest != null ? t.Guest.FullName : "Bilinmiyor",
                        BookingDate = t.TransferDate,
                        Amount = t.FinalPrice,
                        Status = t.Status.ToString()
                    })
                    .ToListAsync();

                var recentBookings = recentCityTours
                    .Concat(recentYachtTours)
                    .Concat(recentTransfers)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(10)
                    .ToList();

                return new DashboardSummaryDto
                {
                    TotalGuests = totalGuests,
                    TotalPersonnel = totalPersonnel,
                    TodayRevenue = todayRevenue,
                    ThisWeekRevenue = thisWeekRevenue,
                    ThisMonthRevenue = thisMonthRevenue,
                    ActiveTransfers = activeTransfers,
                    UpcomingTours = upcomingTours,
                    PendingInvoices = pendingInvoices,
                    TodayBookings = todayBookings,
                    RecentBookings = recentBookings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dashboard özeti getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DailyRevenueDto>> GetDailyRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var dailyRevenues = new List<DailyRevenueDto>();
                var currentDate = start.Date;

                while (currentDate <= end.Date)
                {
                    var dayStart = currentDate;
                    var dayEnd = currentDate.AddDays(1).AddTicks(-1);

                    var cityTourRevenue = await _cityTourRepository.GetAll()
                        .Where(ct => ct.TourDate >= dayStart && ct.TourDate <= dayEnd)
                        .SumAsync(ct => (decimal?)ct.FinalPrice) ?? 0;

                    var yachtTourRevenue = await _yachtTourRepository.GetAll()
                        .Where(yt => yt.TourDate >= dayStart && yt.TourDate <= dayEnd)
                        .SumAsync(yt => (decimal?)yt.FinalPrice) ?? 0;

                    var transferRevenue = await _transferRepository.GetAll()
                        .Where(t => t.TransferDate >= dayStart && t.TransferDate <= dayEnd)
                        .SumAsync(t => (decimal?)t.FinalPrice) ?? 0;

                    var bookingCount = await _cityTourRepository.GetAll()
                        .Where(ct => ct.TourDate >= dayStart && ct.TourDate <= dayEnd).CountAsync()
                        + await _yachtTourRepository.GetAll()
                        .Where(yt => yt.TourDate >= dayStart && yt.TourDate <= dayEnd).CountAsync()
                        + await _transferRepository.GetAll()
                        .Where(t => t.TransferDate >= dayStart && t.TransferDate <= dayEnd).CountAsync();

                    dailyRevenues.Add(new DailyRevenueDto
                    {
                        Date = currentDate,
                        TotalRevenue = cityTourRevenue + yachtTourRevenue + transferRevenue,
                        CityTourRevenue = cityTourRevenue,
                        YachtTourRevenue = yachtTourRevenue,
                        TransferRevenue = transferRevenue,
                        PaymentCount = bookingCount
                    });

                    currentDate = currentDate.AddDays(1);
                }

                return dailyRevenues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Günlük gelir raporu getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<WeeklyRevenueDto>> GetWeeklyRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
                var end = endDate ?? DateTime.UtcNow;

                var weeklyRevenues = new List<WeeklyRevenueDto>();
                var currentDate = start.Date;
                var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;

                while (currentDate <= end.Date)
                {
                    var weekStart = currentDate;
                    // Haftanın başlangıcını Pazartesi'ye ayarla
                    var dayOfWeek = (int)weekStart.DayOfWeek;
                    if (dayOfWeek == 0) dayOfWeek = 7; // Pazar = 7
                    weekStart = weekStart.AddDays(-(dayOfWeek - 1));

                    var weekEnd = weekStart.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);
                    if (weekEnd > end) weekEnd = end;

                    var year = calendar.GetYear(weekStart);
                    var week = calendar.GetWeekOfYear(weekStart, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                    var cityTourRevenue = await _cityTourRepository.GetAll()
                        .Where(ct => ct.TourDate >= weekStart && ct.TourDate <= weekEnd)
                        .SumAsync(ct => (decimal?)ct.FinalPrice) ?? 0;

                    var yachtTourRevenue = await _yachtTourRepository.GetAll()
                        .Where(yt => yt.TourDate >= weekStart && yt.TourDate <= weekEnd)
                        .SumAsync(yt => (decimal?)yt.FinalPrice) ?? 0;

                    var transferRevenue = await _transferRepository.GetAll()
                        .Where(t => t.TransferDate >= weekStart && t.TransferDate <= weekEnd)
                        .SumAsync(t => (decimal?)t.FinalPrice) ?? 0;

                    var bookingCount = await _cityTourRepository.GetAll()
                        .Where(ct => ct.TourDate >= weekStart && ct.TourDate <= weekEnd).CountAsync()
                        + await _yachtTourRepository.GetAll()
                        .Where(yt => yt.TourDate >= weekStart && yt.TourDate <= weekEnd).CountAsync()
                        + await _transferRepository.GetAll()
                        .Where(t => t.TransferDate >= weekStart && t.TransferDate <= weekEnd).CountAsync();

                    weeklyRevenues.Add(new WeeklyRevenueDto
                    {
                        Year = year,
                        Week = week,
                        WeekStart = weekStart,
                        WeekEnd = weekEnd,
                        TotalRevenue = cityTourRevenue + yachtTourRevenue + transferRevenue,
                        CityTourRevenue = cityTourRevenue,
                        YachtTourRevenue = yachtTourRevenue,
                        TransferRevenue = transferRevenue,
                        BookingCount = bookingCount
                    });

                    currentDate = weekEnd.AddDays(1);
                }

                return weeklyRevenues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Haftalık gelir raporu getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<YearlyRevenueDto>> GetYearlyRevenueAsync(int? startYear = null, int? endYear = null)
        {
            try
            {
                var start = startYear ?? DateTime.UtcNow.Year - 5;
                var end = endYear ?? DateTime.UtcNow.Year;

                var yearlyRevenues = new List<YearlyRevenueDto>();

                for (int year = start; year <= end; year++)
                {
                    var yearStart = new DateTime(year, 1, 1);
                    var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);

                    var cityTourRevenue = await _cityTourRepository.GetAll()
                        .Where(ct => ct.TourDate >= yearStart && ct.TourDate <= yearEnd)
                        .SumAsync(ct => (decimal?)ct.FinalPrice) ?? 0;

                    var yachtTourRevenue = await _yachtTourRepository.GetAll()
                        .Where(yt => yt.TourDate >= yearStart && yt.TourDate <= yearEnd)
                        .SumAsync(yt => (decimal?)yt.FinalPrice) ?? 0;

                    var transferRevenue = await _transferRepository.GetAll()
                        .Where(t => t.TransferDate >= yearStart && t.TransferDate <= yearEnd)
                        .SumAsync(t => (decimal?)t.FinalPrice) ?? 0;

                    var bookingCount = await _cityTourRepository.GetAll()
                        .Where(ct => ct.TourDate >= yearStart && ct.TourDate <= yearEnd).CountAsync()
                        + await _yachtTourRepository.GetAll()
                        .Where(yt => yt.TourDate >= yearStart && yt.TourDate <= yearEnd).CountAsync()
                        + await _transferRepository.GetAll()
                        .Where(t => t.TransferDate >= yearStart && t.TransferDate <= yearEnd).CountAsync();

                    var guestCount = await _guestRepository.GetAll()
                        .Where(g => g.CreatedDate >= yearStart && g.CreatedDate <= yearEnd).CountAsync();

                    yearlyRevenues.Add(new YearlyRevenueDto
                    {
                        Year = year,
                        TotalRevenue = cityTourRevenue + yachtTourRevenue + transferRevenue,
                        CityTourRevenue = cityTourRevenue,
                        YachtTourRevenue = yachtTourRevenue,
                        TransferRevenue = transferRevenue,
                        BookingCount = bookingCount,
                        GuestCount = guestCount
                    });
                }

                return yearlyRevenues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yıllık gelir raporu getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PopularTourDto>> GetPopularToursAsync(string? tourType = null, int? limit = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-6);
                var end = endDate ?? DateTime.UtcNow;
                var limitValue = limit ?? 10;

                var popularTours = new List<PopularTourDto>();

                if (string.IsNullOrEmpty(tourType) || tourType.Equals("CityTour", StringComparison.OrdinalIgnoreCase))
                {
                    var cityToursData = await _cityTourRepository.GetAll()
                        .Include(ct => ct.City)
                        .Where(ct => ct.TourDate >= start && ct.TourDate <= end)
                        .ToListAsync();

                    var cityTours = cityToursData
                        .GroupBy(ct => new { 
                            CityId = ct.CityId, 
                            CityName = ct.City != null ? ct.City.CityName : "Bilinmiyor", 
                            Language = ct.Language 
                        })
                        .Select(g => new PopularTourDto
                        {
                            TourType = "CityTour",
                            CityName = g.Key.CityName,
                            Language = g.Key.Language,
                            BookingCount = g.Count(),
                            TotalRevenue = g.Sum(ct => ct.FinalPrice),
                            AveragePrice = g.Average(ct => ct.FinalPrice)
                        })
                        .OrderByDescending(t => t.BookingCount)
                        .Take(limitValue)
                        .ToList();

                    popularTours.AddRange(cityTours);
                }

                if (string.IsNullOrEmpty(tourType) || tourType.Equals("YachtTour", StringComparison.OrdinalIgnoreCase))
                {
                    var yachtToursData = await _yachtTourRepository.GetAll()
                        .Include(yt => yt.City)
                        .Where(yt => yt.TourDate >= start && yt.TourDate <= end)
                        .ToListAsync();

                    var yachtTours = yachtToursData
                        .GroupBy(yt => new { 
                            CityId = yt.CityId, 
                            CityName = yt.City != null ? yt.City.CityName : "Bilinmiyor", 
                            YachtName = yt.YachtName 
                        })
                        .Select(g => new PopularTourDto
                        {
                            TourType = "YachtTour",
                            CityName = g.Key.CityName,
                            YachtName = g.Key.YachtName,
                            BookingCount = g.Count(),
                            TotalRevenue = g.Sum(yt => yt.FinalPrice),
                            AveragePrice = g.Average(yt => yt.FinalPrice)
                        })
                        .OrderByDescending(t => t.BookingCount)
                        .Take(limitValue)
                        .ToList();

                    popularTours.AddRange(yachtTours);
                }

                return popularTours.OrderByDescending(t => t.BookingCount).Take(limitValue).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Popüler turlar getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PersonnelPerformanceDto>> GetPersonnelPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null, string? serviceType = null, int? personnelId = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
                var end = endDate ?? DateTime.UtcNow;

                var transferQuery = _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date >= start.Date && t.TransferDate.Date <= end.Date);
                
                var cityTourQuery = _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date >= start.Date && ct.TourDate.Date <= end.Date);
                
                var yachtTourQuery = _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date >= start.Date && yt.TourDate.Date <= end.Date);

                // Servis tipi filtresi
                if (!string.IsNullOrEmpty(serviceType))
                {
                    if (serviceType == "Transfer")
                    {
                        cityTourQuery = cityTourQuery.Where(_ => false); // Empty query
                        yachtTourQuery = yachtTourQuery.Where(_ => false);
                    }
                    else if (serviceType == "CityTour")
                    {
                        transferQuery = transferQuery.Where(_ => false);
                        yachtTourQuery = yachtTourQuery.Where(_ => false);
                    }
                    else if (serviceType == "YachtTour")
                    {
                        transferQuery = transferQuery.Where(_ => false);
                        cityTourQuery = cityTourQuery.Where(_ => false);
                    }
                }

                // Personel filtresi - eğer belirtilmişse sadece o personeli göster
                if (personnelId.HasValue)
                {
                    transferQuery = transferQuery.Where(t => t.PersonnelId == personnelId.Value);
                    cityTourQuery = cityTourQuery.Where(ct => ct.PersonnelId == personnelId.Value);
                    yachtTourQuery = yachtTourQuery.Where(yt => yt.PersonnelId == personnelId.Value);
                }

                var transferPersonnelIds = await transferQuery
                    .Where(t => t.PersonnelId.HasValue && t.PersonnelId.Value > 0)
                    .Select(t => t.PersonnelId!.Value)
                    .Distinct()
                    .ToListAsync();

                var cityTourPersonnelIds = await cityTourQuery
                    .Where(ct => ct.PersonnelId.HasValue && ct.PersonnelId.Value > 0)
                    .Select(ct => ct.PersonnelId!.Value)
                    .Distinct()
                    .ToListAsync();

                var yachtTourPersonnelIds = await yachtTourQuery
                    .Where(yt => yt.PersonnelId.HasValue && yt.PersonnelId.Value > 0)
                    .Select(yt => yt.PersonnelId!.Value)
                    .Distinct()
                    .ToListAsync();

                var personnelIds = transferPersonnelIds
                    .Concat(cityTourPersonnelIds)
                    .Concat(yachtTourPersonnelIds)
                    .Distinct()
                    .ToList();

                var performances = new List<PersonnelPerformanceDto>();

                foreach (var pid in personnelIds)
                {
                    var personnel = await _personnelRepository.GetByIdAsync(pid);
                    if (personnel == null) continue;

                    var transfers = await transferQuery
                        .Where(t => t.PersonnelId.HasValue && t.PersonnelId.Value == pid)
                        .ToListAsync();

                    var cityTours = await cityTourQuery
                        .Where(ct => ct.PersonnelId.HasValue && ct.PersonnelId.Value == pid)
                        .ToListAsync();

                    var yachtTours = await yachtTourQuery
                        .Where(yt => yt.PersonnelId.HasValue && yt.PersonnelId.Value == pid)
                        .ToListAsync();

                    var totalBookings = transfers.Count + cityTours.Count + yachtTours.Count;
                    var totalRevenue = transfers.Sum(t => t.FinalPrice) +
                                      cityTours.Sum(ct => ct.FinalPrice) +
                                      yachtTours.Sum(yt => yt.FinalPrice);
                    var averageBookingValue = totalBookings > 0 ? totalRevenue / totalBookings : 0;

                    performances.Add(new PersonnelPerformanceDto
                    {
                        PersonnelId = pid,
                        FullName = personnel.FullName,
                        UserType = personnel.UserType.ToString(),
                        TotalBookings = totalBookings,
                        TransferCount = transfers.Count,
                        CityTourCount = cityTours.Count,
                        YachtTourCount = yachtTours.Count,
                        TotalRevenue = totalRevenue,
                        AverageBookingValue = averageBookingValue
                    });
                }

                return performances.OrderByDescending(p => p.TotalRevenue).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Personel performans raporu getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<VatAccrualReportDto> GetVatAccrualReportAsync(DateTime? startDate = null, DateTime? endDate = null, string? currency = null)
        {
            try
            {
                var report = new VatAccrualReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Currency = currency
                };

                // Get invoices in date range
                var invoicesQuery = _invoiceRepository.GetAll();
                
                if (startDate.HasValue)
                    invoicesQuery = invoicesQuery.Where(i => i.IssueDate >= startDate.Value);
                
                if (endDate.HasValue)
                    invoicesQuery = invoicesQuery.Where(i => i.IssueDate <= endDate.Value.AddDays(1).AddTicks(-1));

                var invoices = await invoicesQuery
                    .Include(i => i.InvoiceItems)
                    .ToListAsync();

                report.TotalInvoiceCount = invoices.Count;

                // Get posted journal entries
                var postedJournalEntries = await _journalEntryRepository
                    .GetAll(j => j.InvoiceId != null && invoices.Select(i => i.Id).Contains(j.InvoiceId.Value))
                    .Include(j => j.Lines)
                    .ToListAsync();

                report.PostedJournalCount = postedJournalEntries.Count;

                // Calculate VAT from invoice items
                foreach (var invoice in invoices)
                {
                    if (currency != null && invoice.Currency != currency)
                        continue;

                    var invoiceCurrency = invoice.Currency ?? "TRY";
                    var invoiceVat = invoice.InvoiceItems.Sum(ii => ii.VatAmount);

                    // Add to total VAT by currency
                    if (!report.TotalVatByCurrency.ContainsKey(invoiceCurrency))
                        report.TotalVatByCurrency[invoiceCurrency] = 0m;
                    report.TotalVatByCurrency[invoiceCurrency] += invoiceVat;

                    // Add to VAT by service type
                    foreach (var item in invoice.InvoiceItems)
                    {
                        var serviceType = item.ServiceType ?? "General";
                        if (!report.VatByServiceType.ContainsKey(serviceType))
                            report.VatByServiceType[serviceType] = new Dictionary<string, decimal>();

                        if (!report.VatByServiceType[serviceType].ContainsKey(invoiceCurrency))
                            report.VatByServiceType[serviceType][invoiceCurrency] = 0m;

                        report.VatByServiceType[serviceType][invoiceCurrency] += item.VatAmount;
                    }

                    // Check if VAT is posted (391 account)
                    var isPosted = postedJournalEntries.Any(j => j.InvoiceId == invoice.Id);
                    if (!isPosted)
                    {
                        if (!report.UnpostedVatByCurrency.ContainsKey(invoiceCurrency))
                            report.UnpostedVatByCurrency[invoiceCurrency] = 0m;
                        report.UnpostedVatByCurrency[invoiceCurrency] += invoiceVat;
                    }
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"VAT tahakkuk raporu getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<VatPeriodReportDto>> GetVatPeriodReportAsync(DateTime? startDate = null, DateTime? endDate = null, string? periodType = null, string? currency = null)
        {
            try
            {
                periodType = periodType?.ToLowerInvariant() ?? "monthly"; // monthly, weekly, daily

                // Default date range: current year
                if (!startDate.HasValue)
                    startDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
                
                if (!endDate.HasValue)
                    endDate = DateTime.UtcNow;

                // Get invoices in date range
                var invoicesQuery = _invoiceRepository.GetAll()
                    .Where(i => i.IssueDate >= startDate.Value && i.IssueDate <= endDate.Value.AddDays(1).AddTicks(-1));

                if (currency != null)
                    invoicesQuery = invoicesQuery.Where(i => i.Currency == currency);

                var invoices = await invoicesQuery
                    .Include(i => i.InvoiceItems)
                    .ToListAsync();

                var periodReports = new Dictionary<string, VatPeriodReportDto>();

                foreach (var invoice in invoices)
                {
                    var invoiceCurrency = invoice.Currency ?? "TRY";
                    var invoiceVat = invoice.InvoiceItems.Sum(ii => ii.VatAmount);
                    var invoiceNet = invoice.InvoiceItems.Sum(ii => ii.Amount - ii.VatAmount);
                    var invoiceGross = invoice.TotalAmount;

                    string periodKey;
                    DateTime periodStart;
                    DateTime periodEnd;

                    switch (periodType)
                    {
                        case "daily":
                            periodKey = invoice.IssueDate.ToString("yyyy-MM-dd");
                            periodStart = invoice.IssueDate.Date;
                            periodEnd = periodStart.AddDays(1).AddTicks(-1);
                            break;
                        case "weekly":
                            var weekStart = invoice.IssueDate.Date.AddDays(-(int)invoice.IssueDate.DayOfWeek);
                            periodKey = $"{weekStart:yyyy}-W{System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(weekStart, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday):D2}";
                            periodStart = weekStart;
                            periodEnd = periodStart.AddDays(7).AddTicks(-1);
                            break;
                        case "monthly":
                        default:
                            periodKey = invoice.IssueDate.ToString("yyyy-MM");
                            periodStart = new DateTime(invoice.IssueDate.Year, invoice.IssueDate.Month, 1);
                            periodEnd = periodStart.AddMonths(1).AddTicks(-1);
                            break;
                    }

                    if (!periodReports.ContainsKey(periodKey))
                    {
                        periodReports[periodKey] = new VatPeriodReportDto
                        {
                            Period = periodKey,
                            PeriodStart = periodStart,
                            PeriodEnd = periodEnd,
                            Currency = invoiceCurrency
                        };
                    }

                    var report = periodReports[periodKey];
                    report.TotalVat += invoiceVat;
                    report.TotalNetAmount += invoiceNet;
                    report.TotalGrossAmount += invoiceGross;
                    report.InvoiceCount++;

                    // Add VAT by service type
                    foreach (var item in invoice.InvoiceItems)
                    {
                        var serviceType = item.ServiceType ?? "General";
                        if (!report.VatByServiceType.ContainsKey(serviceType))
                            report.VatByServiceType[serviceType] = 0m;
                        report.VatByServiceType[serviceType] += item.VatAmount;
                    }
                }

                return periodReports.Values.OrderBy(r => r.PeriodStart).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dönem bazlı KDV raporu getirilirken hata: {ex.Message}");
                throw;
            }
        }
        public async Task<string> GenerateWeeklyOperationalReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // 1. Gelir verilerini topla
                var revenueSummary = await GetRevenueSummaryAsync(startDate, endDate);
                var revenueByCategory = new Dictionary<string, decimal>
                {
                    { "Transfer", revenueSummary.TransferRevenueByCurrency.Values.Sum() },
                    { "City Tour", revenueSummary.CityTourRevenueByCurrency.Values.Sum() },
                    { "Yacht Tour", revenueSummary.YachtTourRevenueByCurrency.Values.Sum() }
                };

                // 2. Misafir verileri
                var newGuests = await _guestRepository.GetAll()
                    .Where(g => g.CreatedDate >= startDate && g.CreatedDate <= endDate)
                    .ToListAsync();
                
                var vipGuests = newGuests.Where(g => g.IsSpecialGuest).Count();

                // 3. Memnuniyet verileri
                var reviews = await _reviewRepository.GetAll()
                    .Where(r => r.CreatedDate >= startDate && r.CreatedDate <= endDate)
                    .ToListAsync();
                
                var avgSatisfaction = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;

                // 4. Popüler destinasyonlar
                var popularDests = await GetPopularDestinationsAsync(5);

                // 5. Rapor verisini birleştir
                var reportData = new
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    RevenueByCategory = revenueByCategory,
                    TotalBookings = revenueSummary.TotalBookings,
                    TransferCount = revenueSummary.TransferCount,
                    CityTourCount = revenueSummary.CityTourCount,
                    YachtTourCount = revenueSummary.YachtTourCount,
                    NewGuestCount = newGuests.Count,
                    VIPGuestCount = vipGuests,
                    AverageSatisfaction = avgSatisfaction,
                    PopularDestinations = popularDests
                };

                // 6. AI Insights ekle
                string aiInsight = await GetReportInsightsAsync("WeeklyOperationalReport", reportData);

                // 7. PDF Oluştur (AI özeti ile birlikte)
                return await _pdfService.GenerateWeeklyReportPdfAsync(new { 
                    Data = reportData, 
                    AIInsight = aiInsight 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Haftalık operasyonel rapor oluşturulurken hata oluştu.");
                throw;
            }
        }

        public async Task<string> GetReportInsightsAsync(string reportType, object reportData)
        {
            try
            {
                _logger.LogInformation("Generating AI Insights for report: {ReportType}", reportType);
                
                string dataJson = System.Text.Json.JsonSerializer.Serialize(reportData);
                
                var request = new AIChatRequest
                {
                    Message = $"Analyze this '{reportType}' report data and provide 3-4 professional executive insights, potential risks, and optimization opportunities. Return the results in Turkish. Data: {dataJson}",
                    Context = $"ReportType: {reportType}\nGenerationDate: {DateTime.UtcNow}"
                };

                var response = await _aiAssistantService.ProcessMessageAsync(request);
                return response.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate AI report insights for {ReportType}", reportType);
                return "Rapor analizi şu an yapılamıyor. Lütfen daha sonra tekrar deneyiniz.";
            }
        }
    }
}

