using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Calendar
{
    /// <summary>
    /// Takvim servisi interface'i
    /// </summary>
    public interface ICalendarService
    {
        /// <summary>
        /// Transfer için iCal/ICS formatında takvim event'i oluşturur
        /// </summary>
        Task<CalendarExportResult> GenerateTransferCalendarAsync(int transferId);

        /// <summary>
        /// Şehir turu için iCal/ICS formatında takvim event'i oluşturur
        /// </summary>
        Task<CalendarExportResult> GenerateCityTourCalendarAsync(int cityTourId);

        /// <summary>
        /// Yat turu için iCal/ICS formatında takvim event'i oluşturur
        /// </summary>
        Task<CalendarExportResult> GenerateYachtTourCalendarAsync(int yachtTourId);

        /// <summary>
        /// Rezervasyon için iCal/ICS formatında takvim event'i oluşturur
        /// </summary>
        Task<CalendarExportResult> GenerateReservationCalendarAsync(int reservationId);

        /// <summary>
        /// Birden fazla transfer için toplu takvim dosyası oluşturur
        /// </summary>
        Task<CalendarExportResult> GenerateBulkTransferCalendarAsync(List<int> transferIds, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Birden fazla tur için toplu takvim dosyası oluşturur
        /// </summary>
        Task<CalendarExportResult> GenerateBulkTourCalendarAsync(List<int> cityTourIds, List<int> yachtTourIds, DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// Takvim dışa aktarma sonucu
    /// </summary>
    public class CalendarExportResult
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text/calendar; charset=utf-8";
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

