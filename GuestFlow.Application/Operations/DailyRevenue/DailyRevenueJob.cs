using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyRevenue
{
    /// <summary>
    /// Günlük gelir hesaplama servisi - PaymentEntity'den tahsilat bazlı hesaplama
    /// Gelir = Tamamlanmış ödemeler (PaymentStatus.Completed)
    /// </summary>
    public class DailyRevenueJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IRepository<DailyRevenueEntity> _dailyRevenueRepository;
        private readonly ILogger<DailyRevenueJob> _logger;

        public DailyRevenueJob(
            IUnitOfWork unitOfWork,
            IRepository<PaymentEntity> paymentRepository,
            IRepository<DailyRevenueEntity> dailyRevenueRepository,
            ILogger<DailyRevenueJob> logger)
        {
            _unitOfWork = unitOfWork;
            _paymentRepository = paymentRepository;
            _dailyRevenueRepository = dailyRevenueRepository;
            _logger = logger;
        }

        /// <summary>
        /// Belirli bir gün için tahsilat bazlı geliri hesaplar (currency bazlı)
        /// Kaynak: PaymentEntity (Status = Completed, PaymentDate = date)
        /// </summary>
        public async Task CalculateDailyRevenue(DateTime date)
        {
            try
            {
                _logger.LogInformation($"Günlük gelir hesaplaması başlıyor ({date:yyyy-MM-dd})...");

                // O gün için tamamlanmış ödemeleri çek
                var completedPayments = await _paymentRepository.GetAll()
                    .Where(p => p.PaymentDate.Date == date.Date && 
                               p.Status == PaymentStatus.Completed && 
                               !p.IsDeleted)
                    .ToListAsync();

                // O gün için iade edilen ödemeleri çek
                var refundedPayments = await _paymentRepository.GetAll()
                    .Where(p => p.RefundDate.HasValue && 
                               p.RefundDate.Value.Date == date.Date && 
                               p.Status == PaymentStatus.Refunded && 
                               !p.IsDeleted)
                    .ToListAsync();

                // Currency bazlı grupla
                var currencies = completedPayments.Select(p => p.Currency)
                    .Union(refundedPayments.Select(p => p.Currency))
                    .Distinct()
                    .ToList();

                // Her currency için ayrı kayıt oluştur
                foreach (var currency in currencies)
                {
                    var currencyPayments = completedPayments.Where(p => p.Currency == currency).ToList();
                    var currencyRefunds = refundedPayments.Where(p => p.Currency == currency).ToList();

                    // Servis bazlı gelir hesapla
                    var transferRevenue = currencyPayments
                        .Where(p => p.TransferId.HasValue)
                        .Sum(p => p.Amount);

                    var cityTourRevenue = currencyPayments
                        .Where(p => p.CityTourId.HasValue)
                        .Sum(p => p.Amount);

                    var yachtTourRevenue = currencyPayments
                        .Where(p => p.YachtTourId.HasValue)
                        .Sum(p => p.Amount);

                    var generalRevenue = currencyPayments
                        .Where(p => !p.TransferId.HasValue && !p.CityTourId.HasValue && !p.YachtTourId.HasValue)
                        .Sum(p => p.Amount);

                    var totalRevenue = transferRevenue + cityTourRevenue + yachtTourRevenue + generalRevenue;
                    var refundedAmount = currencyRefunds.Sum(p => p.Amount);
                    var netRevenue = totalRevenue - refundedAmount;
                    var paymentCount = currencyPayments.Count;

                    _logger.LogInformation($"[{currency}] Transfer: {transferRevenue}, CityTour: {cityTourRevenue}, YachtTour: {yachtTourRevenue}, General: {generalRevenue}, Refund: {refundedAmount}, Net: {netRevenue}");

                    // Bu tarih + currency için mevcut kayıt var mı?
                    var existingRevenue = await _dailyRevenueRepository.GetAsync(
                        dr => dr.Date.Date == date.Date && dr.Currency == currency);

                    if (existingRevenue != null)
                    {
                        // Güncelle
                        existingRevenue.TransferRevenue = transferRevenue;
                        existingRevenue.CityTourRevenue = cityTourRevenue;
                        existingRevenue.YachtTourRevenue = yachtTourRevenue;
                        existingRevenue.GeneralRevenue = generalRevenue;
                        existingRevenue.TotalRevenue = totalRevenue;
                        existingRevenue.RefundedAmount = refundedAmount;
                        existingRevenue.NetRevenue = netRevenue;
                        existingRevenue.PaymentCount = paymentCount;

                        await _dailyRevenueRepository.UpdateAsync(existingRevenue);
                        _logger.LogInformation($"Mevcut günlük gelir güncellendi ({date:yyyy-MM-dd} - {currency}): Net {netRevenue}");
                    }
                    else
                    {
                        // Yeni kayıt oluştur
                        var dailyRevenue = new DailyRevenueEntity
                        {
                            Date = date.Date,
                            Currency = currency,
                            TransferRevenue = transferRevenue,
                            CityTourRevenue = cityTourRevenue,
                            YachtTourRevenue = yachtTourRevenue,
                            GeneralRevenue = generalRevenue,
                            TotalRevenue = totalRevenue,
                            RefundedAmount = refundedAmount,
                            NetRevenue = netRevenue,
                            PaymentCount = paymentCount,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        };

                        await _dailyRevenueRepository.AddAsync(dailyRevenue);
                        _logger.LogInformation($"Yeni günlük gelir eklendi ({date:yyyy-MM-dd} - {currency}): Net {netRevenue}");
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Günlük gelir hesaplandı ({date:yyyy-MM-dd}): {currencies.Count} para birimi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Günlük gelir hesaplanırken hata çıktı ({date:yyyy-MM-dd}): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Belirli bir tarih aralığı için günlük gelirleri hesaplar
        /// </summary>
        public async Task CalculateDailyRevenueForRange(DateTime startDate, DateTime endDate)
        {
            var currentDate = startDate.Date;
            while (currentDate <= endDate.Date)
            {
                await CalculateDailyRevenue(currentDate);
                currentDate = currentDate.AddDays(1);
            }
        }

        /// <summary>
        /// Yeni bir ödeme eklendiğinde günlük geliri günceller (gerçek zamanlı)
        /// </summary>
        public async Task UpdateDailyRevenueOnPayment(DateTime paymentDate, string currency, decimal amount, string? serviceType)
        {
            try
            {
                var existingRevenue = await _dailyRevenueRepository.GetAsync(
                    dr => dr.Date.Date == paymentDate.Date && dr.Currency == currency);

                if (existingRevenue == null)
                {
                    existingRevenue = new DailyRevenueEntity
                    {
                        Date = paymentDate.Date,
                        Currency = currency,
                        TransferRevenue = 0,
                        CityTourRevenue = 0,
                        YachtTourRevenue = 0,
                        GeneralRevenue = 0,
                        TotalRevenue = 0,
                        RefundedAmount = 0,
                        NetRevenue = 0,
                        PaymentCount = 0,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _dailyRevenueRepository.AddAsync(existingRevenue);
                }

                // Servis tipine göre ilgili alanı güncelle
                switch (serviceType?.ToLower())
                {
                    case "transfer":
                        existingRevenue.TransferRevenue += amount;
                        break;
                    case "citytour":
                        existingRevenue.CityTourRevenue += amount;
                        break;
                    case "yachttour":
                        existingRevenue.YachtTourRevenue += amount;
                        break;
                    default:
                        existingRevenue.GeneralRevenue += amount;
                        break;
                }

                existingRevenue.TotalRevenue += amount;
                existingRevenue.NetRevenue = existingRevenue.TotalRevenue - existingRevenue.RefundedAmount;
                existingRevenue.PaymentCount += 1;

                await _dailyRevenueRepository.UpdateAsync(existingRevenue);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Günlük gelir güncellendi ({paymentDate:yyyy-MM-dd} - {currency}): +{amount} ({serviceType ?? "General"})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Günlük gelir güncellenirken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// İade işleminde günlük geliri günceller
        /// </summary>
        public async Task UpdateDailyRevenueOnRefund(DateTime refundDate, string currency, decimal amount)
        {
            try
            {
                var existingRevenue = await _dailyRevenueRepository.GetAsync(
                    dr => dr.Date.Date == refundDate.Date && dr.Currency == currency);

                if (existingRevenue == null)
                {
                    existingRevenue = new DailyRevenueEntity
                    {
                        Date = refundDate.Date,
                        Currency = currency,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _dailyRevenueRepository.AddAsync(existingRevenue);
                }

                existingRevenue.RefundedAmount += amount;
                existingRevenue.NetRevenue = existingRevenue.TotalRevenue - existingRevenue.RefundedAmount;

                await _dailyRevenueRepository.UpdateAsync(existingRevenue);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"İade kaydedildi ({refundDate:yyyy-MM-dd} - {currency}): -{amount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"İade kaydedilirken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Belirli bir tarih aralığı için toplam geliri döndürür (currency bazlı)
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetTotalRevenueForRange(DateTime startDate, DateTime endDate)
        {
            var revenues = await _dailyRevenueRepository.GetAll()
                .Where(dr => dr.Date.Date >= startDate.Date && dr.Date.Date <= endDate.Date && !dr.IsDeleted)
                .GroupBy(dr => dr.Currency)
                .Select(g => new { Currency = g.Key, Total = g.Sum(dr => dr.NetRevenue) })
                .ToListAsync();

            return revenues.ToDictionary(r => r.Currency, r => r.Total);
        }

        /// <summary>
        /// Belirli bir gün için geliri döndürür (currency bazlı)
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetDailyRevenue(DateTime date)
        {
            return await GetTotalRevenueForRange(date, date);
        }
    }
}