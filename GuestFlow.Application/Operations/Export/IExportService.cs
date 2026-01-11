using GuestFlow.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Export
{
    /// <summary>
    /// Dışa aktarma servisi interface'i
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Misafir listesini Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportGuestsToExcelAsync(GuestFilterParameters? filters = null);

        /// <summary>
        /// Misafir listesini CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportGuestsToCsvAsync(GuestFilterParameters? filters = null);

        /// <summary>
        /// Fatura listesini Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportInvoicesToExcelAsync(InvoiceFilterParameters? filters = null);

        /// <summary>
        /// Fatura listesini CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportInvoicesToCsvAsync(InvoiceFilterParameters? filters = null);

        /// <summary>
        /// Gelir raporunu Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportRevenueReportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gelir raporunu CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportRevenueReportToCsvAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Transfer listesini Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportTransfersToExcelAsync(TransferFilterParameters? filters = null);

        /// <summary>
        /// Transfer listesini CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportTransfersToCsvAsync(TransferFilterParameters? filters = null);

        /// <summary>
        /// Journal kayıtlarını (posting date aralığına göre) CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportJournalToCsvAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Journal kayıtlarını (posting date aralığına göre) Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportJournalToExcelAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// Dışa aktarma sonucu
    /// </summary>
    public class ExportResult
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

