using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Sms.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Sms
{
    /// <summary>
    /// SMS servisi interface'i
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// SMS gönderir
        /// </summary>
        Task<ServiceMessage<GetSmsHistoryDto>> SendSmsAsync(SendSmsDto smsDto);

        /// <summary>
        /// Transfer hatırlatma SMS'i gönderir
        /// </summary>
        Task<ServiceMessage<GetSmsHistoryDto>> SendTransferReminderAsync(int transferId, int hoursBefore = 24);

        /// <summary>
        /// Tur hatırlatma SMS'i gönderir
        /// </summary>
        Task<ServiceMessage<GetSmsHistoryDto>> SendTourReminderAsync(string tourType, int tourId, int hoursBefore = 24);

        /// <summary>
        /// Rezervasyon onay SMS'i gönderir
        /// </summary>
        Task<ServiceMessage<GetSmsHistoryDto>> SendReservationConfirmationAsync(int reservationId);

        /// <summary>
        /// SMS geçmişini getirir
        /// </summary>
        Task<GetSmsHistoryDto?> GetSmsHistoryByIdAsync(int id);

        /// <summary>
        /// Sayfalanmış SMS geçmişini getirir
        /// </summary>
        Task<PagedResult<GetSmsHistoryDto>> GetSmsHistoryPagedAsync(int pageNumber, int pageSize, SmsFilterParameters? filters = null, SortingParameters? sorting = null);

        /// <summary>
        /// Misafire gönderilen SMS'leri getirir
        /// </summary>
        Task<List<GetSmsHistoryDto>> GetSmsHistoryByGuestIdAsync(int guestId);

        /// <summary>
        /// Duruma göre SMS'leri getirir
        /// </summary>
        Task<List<GetSmsHistoryDto>> GetSmsHistoryByStatusAsync(string status);

        /// <summary>
        /// SMS durumunu günceller (gateway callback için)
        /// </summary>
        Task<ServiceMessage> UpdateSmsStatusAsync(int smsId, string status, string? messageId = null, string? gatewayResponse = null);

        /// <summary>
        /// SMS istatistiklerini getirir
        /// </summary>
        Task<SmsStatisticsDto> GetSmsStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// SMS istatistikleri DTO'su
    /// </summary>
    public class SmsStatisticsDto
    {
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalFailed { get; set; }
        public int TotalPending { get; set; }
        public decimal SuccessRate { get; set; }
        public Dictionary<string, int> SmsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> SmsByStatus { get; set; } = new Dictionary<string, int>();
    }
}

