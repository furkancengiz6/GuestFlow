using GuestFlow.Application.Operations.Email.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Email
{
    public interface IEmailStatisticsService
    {
        Task<ServiceMessage<EmailStatisticsDto>> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    public class EmailStatisticsService : IEmailStatisticsService
    {
        private readonly IRepository<EmailHistoryEntity> _historyRepository;
        private readonly IRepository<EmailQueueEntity> _queueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailStatisticsService> _logger;

        public EmailStatisticsService(
            IRepository<EmailHistoryEntity> historyRepository,
            IRepository<EmailQueueEntity> queueRepository,
            IUnitOfWork unitOfWork,
            ILogger<EmailStatisticsService> logger)
        {
            _historyRepository = historyRepository;
            _queueRepository = queueRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceMessage<EmailStatisticsDto>> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var statistics = new EmailStatisticsDto();

                // Tarih filtresi
                var historyQuery = _historyRepository.GetAll();
                if (startDate.HasValue)
                    historyQuery = historyQuery.Where(e => e.SentDate >= startDate.Value);
                if (endDate.HasValue)
                    historyQuery = historyQuery.Where(e => e.SentDate <= endDate.Value);

                var queueQuery = _queueRepository.GetAll();

                // Gönderilen e-postalar
                var sentEmails = await historyQuery
                    .Where(e => e.Status == "Sent")
                    .ToListAsync();

                statistics.TotalSent = sentEmails.Count;

                // Başarısız e-postalar
                var failedEmails = await historyQuery
                    .Where(e => e.Status == "Failed")
                    .ToListAsync();

                statistics.TotalFailed = failedEmails.Count;

                // Bekleyen e-postalar
                statistics.TotalPending = await queueQuery
                    .CountAsync(e => e.Status == "Pending");

                // Kuyruktaki toplam
                statistics.TotalInQueue = await queueQuery
                    .CountAsync(e => e.Status == "Pending" || e.Status == "Processing");

                // Başarı oranı
                var total = statistics.TotalSent + statistics.TotalFailed;
                statistics.SuccessRate = total > 0 
                    ? (decimal)statistics.TotalSent / total * 100 
                    : 0;

                // Günlere göre gönderim
                var sentByDay = sentEmails
                    .GroupBy(e => e.SentDate.Date.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.Count());
                statistics.SentByDay = sentByDay;

                // Şablonlara göre gönderim
                var sentByTemplate = sentEmails
                    .Where(e => !string.IsNullOrEmpty(e.TemplateName))
                    .GroupBy(e => e.TemplateName!)
                    .ToDictionary(g => g.Key, g => g.Count());
                statistics.SentByTemplate = sentByTemplate;

                // Başarısızlık nedenlerine göre
                var failedByReason = failedEmails
                    .Where(e => !string.IsNullOrEmpty(e.ErrorMessage))
                    .GroupBy(e => e.ErrorMessage!.Substring(0, Math.Min(50, e.ErrorMessage.Length)))
                    .ToDictionary(g => g.Key, g => g.Count());
                statistics.FailedByReason = failedByReason;

                // Ortalama teslimat süresi (kuyruk kayıtlarından)
                var processedEmails = await queueQuery
                    .Where(e => e.Status == "Sent" && e.SentDate.HasValue && e.CreatedDate != null)
                    .ToListAsync();

                if (processedEmails.Any())
                {
                    var avgDeliveryTime = processedEmails
                        .Select(e => (e.SentDate!.Value - e.CreatedDate).TotalSeconds)
                        .Average();
                    statistics.AverageDeliveryTime = (int)avgDeliveryTime;
                }

                // Açılma istatistikleri
                var openedEmails = await historyQuery
                    .Where(e => e.IsOpened)
                    .CountAsync();
                statistics.TotalOpened = openedEmails;
                statistics.OpenRate = statistics.TotalSent > 0 
                    ? (decimal)openedEmails / statistics.TotalSent * 100 
                    : 0;

                // Tıklama istatistikleri
                var totalClicks = await historyQuery
                    .SumAsync(e => e.ClickCount);
                statistics.TotalClicks = totalClicks;
                statistics.ClickRate = statistics.TotalSent > 0 
                    ? (decimal)totalClicks / statistics.TotalSent * 100 
                    : 0;

                return new ServiceMessage<EmailStatisticsDto>
                {
                    IsSuccess = true,
                    Message = "E-posta istatistikleri başarıyla getirildi.",
                    Data = statistics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta istatistikleri getirilirken hata: {ex.Message}");
                return new ServiceMessage<EmailStatisticsDto>
                {
                    IsSuccess = false,
                    Message = $"E-posta istatistikleri getirilirken hata oluştu: {ex.Message}",
                    Data = new EmailStatisticsDto()
                };
            }
        }
    }
}

