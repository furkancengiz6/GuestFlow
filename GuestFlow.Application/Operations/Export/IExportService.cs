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

        /// <summary>
        /// Guest Ledger - Misafir bazlı tüm finansal hareketleri CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportGuestLedgerToCsvAsync(int? guestId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Guest Ledger - Misafir bazlı tüm finansal hareketleri Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportGuestLedgerToExcelAsync(int? guestId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Supplier Ledger - Tedarikçi bazlı tüm finansal hareketleri CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportSupplierLedgerToCsvAsync(int? supplierId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Supplier Ledger - Tedarikçi bazlı tüm finansal hareketleri Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportSupplierLedgerToExcelAsync(int? supplierId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Room Ledger - Oda bazlı tüm işlemleri CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportRoomLedgerToCsvAsync(string? roomNumber = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Room Ledger - Oda bazlı tüm işlemleri Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportRoomLedgerToExcelAsync(string? roomNumber = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// VAT tahakkuk raporunu (391 hesabı) Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportVatAccrualReportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null, string? currency = null);

        /// <summary>
        /// VAT tahakkuk raporunu (391 hesabı) CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportVatAccrualReportToCsvAsync(DateTime? startDate = null, DateTime? endDate = null, string? currency = null);

        /// <summary>
        /// Dönem bazlı KDV raporunu Excel formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportVatPeriodReportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null, string? periodType = null, string? currency = null);

        /// <summary>
        /// Dönem bazlı KDV raporunu CSV formatında dışa aktarır
        /// </summary>
        Task<ExportResult> ExportVatPeriodReportToCsvAsync(DateTime? startDate = null, DateTime? endDate = null, string? periodType = null, string? currency = null);
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

