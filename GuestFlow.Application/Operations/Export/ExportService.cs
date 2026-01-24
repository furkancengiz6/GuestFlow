using ClosedXML.Excel;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Reports;
using GuestFlow.Application.Operations.Transfer;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Export
{
    public class ExportService : IExportService
    {
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<DailyRevenueEntity> _dailyRevenueRepository;
        private readonly IRepository<JournalEntry> _journalEntryRepository;
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<GuestFlow.Domain.Entities.Core.Supplier> _supplierRepository;
        private readonly IRepository<SupplierCost> _supplierCostRepository;
        private readonly IRepository<RoomAssignmentEntity> _roomAssignmentRepository;
        private readonly IReportsService _reportsService;
        private readonly ILogger<ExportService> _logger;

        public ExportService(
            IRepository<GuestEntity> guestRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<DailyRevenueEntity> dailyRevenueRepository,
            IRepository<JournalEntry> journalEntryRepository,
            IRepository<PaymentEntity> paymentRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<GuestFlow.Domain.Entities.Core.Supplier> supplierRepository,
            IRepository<SupplierCost> supplierCostRepository,
            IRepository<RoomAssignmentEntity> roomAssignmentRepository,
            IReportsService reportsService,
            ILogger<ExportService> logger)
        {
            _guestRepository = guestRepository;
            _invoiceRepository = invoiceRepository;
            _transferRepository = transferRepository;
            _dailyRevenueRepository = dailyRevenueRepository;
            _journalEntryRepository = journalEntryRepository;
            _paymentRepository = paymentRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _supplierRepository = supplierRepository;
            _supplierCostRepository = supplierCostRepository;
            _roomAssignmentRepository = roomAssignmentRepository;
            _reportsService = reportsService;
            _logger = logger;
        }

        public async Task<ExportResult> ExportGuestsToExcelAsync(GuestFilterParameters? filters = null)
        {
            try
            {
                var query = _guestRepository.GetAll(x => !x.IsDeleted).AsQueryable();
                query = query.ApplyGuestFilters(filters);

                var guests = await query
                    .OrderBy(g => g.FullName)
                    .ToListAsync();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Misafirler");

                // Başlıklar
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Misafir Kodu";
                worksheet.Cell(1, 3).Value = "Ad Soyad";
                worksheet.Cell(1, 4).Value = "E-posta";
                worksheet.Cell(1, 5).Value = "Telefon";
                worksheet.Cell(1, 6).Value = "Uyruk";
                worksheet.Cell(1, 7).Value = "Özel Misafir";
                worksheet.Cell(1, 8).Value = "Oluşturulma Tarihi";

                // Başlık stilini ayarla
                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Veriler
                int row = 2;
                foreach (var guest in guests)
                {
                    worksheet.Cell(row, 1).Value = guest.Id;
                    worksheet.Cell(row, 2).Value = guest.GuestCode;
                    worksheet.Cell(row, 3).Value = guest.FullName;
                    worksheet.Cell(row, 4).Value = guest.Email;
                    worksheet.Cell(row, 5).Value = guest.PhoneNumber;
                    worksheet.Cell(row, 6).Value = guest.Nationality;
                    worksheet.Cell(row, 7).Value = guest.IsSpecialGuest ? "Evet" : "Hayır";
                    worksheet.Cell(row, 8).Value = guest.CreatedDate.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    row++;
                }

                // Sütun genişliklerini ayarla
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Misafirler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return new ExportResult
                {
                    FileContent = stream.ToArray(),
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir listesi Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportGuestsToCsvAsync(GuestFilterParameters? filters = null)
        {
            try
            {
                var query = _guestRepository.GetAll(x => !x.IsDeleted).AsQueryable();
                query = query.ApplyGuestFilters(filters);

                var guests = await query
                    .OrderBy(g => g.FullName)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("ID,Misafir Kodu,Ad Soyad,E-posta,Telefon,Uyruk,Özel Misafir,Oluşturulma Tarihi");

                foreach (var guest in guests)
                {
                    csv.AppendLine($"{guest.Id}," +
                        $"{EscapeCsvValue(guest.GuestCode)}," +
                        $"{EscapeCsvValue(guest.FullName)}," +
                        $"{EscapeCsvValue(guest.Email)}," +
                        $"{EscapeCsvValue(guest.PhoneNumber)}," +
                        $"{EscapeCsvValue(guest.Nationality)}," +
                        $"{(guest.IsSpecialGuest ? "Evet" : "Hayır")}," +
                        $"{guest.CreatedDate:dd.MM.yyyy HH:mm}");
                }

                var fileName = $"Misafirler_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var content = Encoding.UTF8.GetBytes(csv.ToString());

                return new ExportResult
                {
                    FileContent = content,
                    FileName = fileName,
                    ContentType = "text/csv; charset=utf-8",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir listesi CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportInvoicesToExcelAsync(InvoiceFilterParameters? filters = null)
        {
            try
            {
                var query = _invoiceRepository.GetAll(x => !x.IsDeleted)
                    .Include(i => i.Guest)
                    .AsQueryable();
                query = query.ApplyInvoiceFilters(filters);

                var invoices = await query
                    .OrderByDescending(i => i.IssueDate)
                    .ToListAsync();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Faturalar");

                // Başlıklar
                worksheet.Cell(1, 1).Value = "Fatura No";
                worksheet.Cell(1, 2).Value = "Misafir";
                worksheet.Cell(1, 3).Value = "Tutar";
                worksheet.Cell(1, 4).Value = "Para Birimi";
                worksheet.Cell(1, 5).Value = "Tarih";
                worksheet.Cell(1, 6).Value = "Hizmetler";
                worksheet.Cell(1, 7).Value = "Notlar";

                // Başlık stilini ayarla
                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Veriler
                int row = 2;
                foreach (var invoice in invoices)
                {
                    var services = invoice.InvoiceItems != null && invoice.InvoiceItems.Any()
                        ? string.Join("; ", invoice.InvoiceItems.Select(i => $"{i.ServiceType}:{i.ServiceId}"))
                        : "";

                    worksheet.Cell(row, 1).Value = invoice.InvoiceNumber;
                    worksheet.Cell(row, 2).Value = invoice.Guest?.FullName ?? "Bilinmiyor";
                    worksheet.Cell(row, 3).Value = invoice.TotalAmount;
                    worksheet.Cell(row, 4).Value = invoice.Currency;
                    worksheet.Cell(row, 5).Value = invoice.IssueDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    worksheet.Cell(row, 6).Value = services;
                    worksheet.Cell(row, 7).Value = invoice.Notes ?? "";
                    row++;
                }

                // Sütun genişliklerini ayarla
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Faturalar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return new ExportResult
                {
                    FileContent = stream.ToArray(),
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura listesi Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportInvoicesToCsvAsync(InvoiceFilterParameters? filters = null)
        {
            try
            {
                var query = _invoiceRepository.GetAll(x => !x.IsDeleted)
                    .Include(i => i.Guest)
                    .AsQueryable();
                query = query.ApplyInvoiceFilters(filters);

                var invoices = await query
                    .OrderByDescending(i => i.IssueDate)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Fatura No,Misafir,Tutar,Para Birimi,Tarih,Hizmetler,Notlar");

                foreach (var invoice in invoices)
                {
                    var services = invoice.InvoiceItems != null && invoice.InvoiceItems.Any()
                        ? string.Join("; ", invoice.InvoiceItems.Select(i => $"{i.ServiceType}:{i.ServiceId}"))
                        : "";

                    csv.AppendLine($"{invoice.InvoiceNumber}," +
                        $"{EscapeCsvValue(invoice.Guest?.FullName ?? "Bilinmiyor")}," +
                        $"{invoice.TotalAmount}," +
                        $"{invoice.Currency}," +
                        $"{invoice.IssueDate:dd.MM.yyyy}," +
                        $"{EscapeCsvValue(services)}," +
                        $"{EscapeCsvValue(invoice.Notes ?? "")}");
                }

                var fileName = $"Faturalar_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var content = Encoding.UTF8.GetBytes(csv.ToString());

                return new ExportResult
                {
                    FileContent = content,
                    FileName = fileName,
                    ContentType = "text/csv; charset=utf-8",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura listesi CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportRevenueReportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var revenueSummary = await _reportsService.GetRevenueSummaryAsync(startDate, endDate);
                var dailyRevenues = await _reportsService.GetDailyRevenueAsync(startDate, endDate);

                using var workbook = new XLWorkbook();

                // Özet sayfası
                var summarySheet = workbook.Worksheets.Add("Gelir Özeti");
                summarySheet.Cell(1, 1).Value = "Toplam Gelir";
                // Use TotalRevenueByCurrency - default to TRY or first currency
                var defaultCurrency = revenueSummary.TotalRevenueByCurrency.Keys.FirstOrDefault() ?? "TRY";
                summarySheet.Cell(1, 2).Value = revenueSummary.TotalRevenueByCurrency.GetValueOrDefault(defaultCurrency, 0);
                summarySheet.Cell(2, 1).Value = "Para Birimi";
                summarySheet.Cell(2, 2).Value = "TRY";
                summarySheet.Cell(3, 1).Value = "Başlangıç Tarihi";
                summarySheet.Cell(3, 2).Value = startDate?.ToString("dd.MM.yyyy") ?? "Tümü";
                summarySheet.Cell(4, 1).Value = "Bitiş Tarihi";
                summarySheet.Cell(4, 2).Value = endDate?.ToString("dd.MM.yyyy") ?? "Tümü";

                // Günlük gelirler sayfası
                var dailySheet = workbook.Worksheets.Add("Günlük Gelirler");
                dailySheet.Cell(1, 1).Value = "Tarih";
                dailySheet.Cell(1, 2).Value = "Gelir";
                dailySheet.Cell(1, 3).Value = "Para Birimi";

                var headerRange = dailySheet.Range(1, 1, 1, 3);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 2;
                foreach (var daily in dailyRevenues)
                {
                    dailySheet.Cell(row, 1).Value = daily.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    dailySheet.Cell(row, 2).Value = daily.TotalRevenue;
                    dailySheet.Cell(row, 3).Value = "TRY";
                    row++;
                }

                dailySheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Gelir_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return new ExportResult
                {
                    FileContent = stream.ToArray(),
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Gelir raporu Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportRevenueReportToCsvAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var revenueSummary = await _reportsService.GetRevenueSummaryAsync(startDate, endDate);
                var dailyRevenues = await _reportsService.GetDailyRevenueAsync(startDate, endDate);

                var csv = new StringBuilder();
                csv.AppendLine("Gelir Raporu");
                // Use TotalRevenueByCurrency - default to TRY or first currency
                var defaultCurrency = revenueSummary.TotalRevenueByCurrency.Keys.FirstOrDefault() ?? "TRY";
                csv.AppendLine($"Toplam Gelir ({defaultCurrency}),{revenueSummary.TotalRevenueByCurrency.GetValueOrDefault(defaultCurrency, 0)}");
                csv.AppendLine($"Para Birimi,{defaultCurrency}");
                csv.AppendLine($"Başlangıç Tarihi,{startDate?.ToString("dd.MM.yyyy") ?? "Tümü"}");
                csv.AppendLine($"Bitiş Tarihi,{endDate?.ToString("dd.MM.yyyy") ?? "Tümü"}");
                csv.AppendLine();
                csv.AppendLine("Tarih,Gelir,Para Birimi");

                foreach (var daily in dailyRevenues)
                {
                    // DailyRevenueDto.TotalRevenue is not deprecated, it's single currency
                    csv.AppendLine($"{daily.Date:dd.MM.yyyy},{daily.TotalRevenue},{daily.Currency}");
                }

                var fileName = $"Gelir_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var content = Encoding.UTF8.GetBytes(csv.ToString());

                return new ExportResult
                {
                    FileContent = content,
                    FileName = fileName,
                    ContentType = "text/csv; charset=utf-8",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Gelir raporu CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportTransfersToExcelAsync(TransferFilterParameters? filters = null)
        {
            try
            {
                var query = _transferRepository.GetAll(x => !x.IsDeleted)
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Include(t => t.Vehicle)
                    .AsQueryable();
                query = query.ApplyTransferFilters(filters);

                var transfers = await query
                    .OrderByDescending(t => t.TransferDate)
                    .ToListAsync();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Transferler");

                // Başlıklar
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Misafir";
                worksheet.Cell(1, 3).Value = "Personel";
                worksheet.Cell(1, 4).Value = "Transfer Tarihi";
                worksheet.Cell(1, 5).Value = "Kalkış Adresi";
                worksheet.Cell(1, 6).Value = "Varış Adresi";
                worksheet.Cell(1, 7).Value = "Fiyat";
                worksheet.Cell(1, 8).Value = "Para Birimi";
                worksheet.Cell(1, 9).Value = "Durum";

                // Başlık stilini ayarla
                var headerRange = worksheet.Range(1, 1, 1, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Veriler
                int row = 2;
                foreach (var transfer in transfers)
                {
                    worksheet.Cell(row, 1).Value = transfer.Id;
                    worksheet.Cell(row, 2).Value = transfer.Guest?.FullName ?? "Bilinmiyor";
                    worksheet.Cell(row, 3).Value = transfer.Personnel?.FullName ?? "Bilinmiyor";
                    worksheet.Cell(row, 4).Value = transfer.TransferDate.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    worksheet.Cell(row, 5).Value = transfer.PickupAddress;
                    worksheet.Cell(row, 6).Value = transfer.DropoffAddress;
                    worksheet.Cell(row, 7).Value = transfer.FinalPrice;
                    worksheet.Cell(row, 8).Value = transfer.Currency;
                    worksheet.Cell(row, 9).Value = transfer.Status;
                    row++;
                }

                // Sütun genişliklerini ayarla
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Transferler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return new ExportResult
                {
                    FileContent = stream.ToArray(),
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer listesi Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportTransfersToCsvAsync(TransferFilterParameters? filters = null)
        {
            try
            {
                var query = _transferRepository.GetAll(x => !x.IsDeleted)
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .AsQueryable();
                query = query.ApplyTransferFilters(filters);

                var transfers = await query
                    .OrderByDescending(t => t.TransferDate)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("ID,Misafir,Personel,Transfer Tarihi,Kalkış Adresi,Varış Adresi,Fiyat,Para Birimi,Durum");

                foreach (var transfer in transfers)
                {
                    csv.AppendLine($"{transfer.Id}," +
                        $"{EscapeCsvValue(transfer.Guest?.FullName ?? "Bilinmiyor")}," +
                        $"{EscapeCsvValue(transfer.Personnel?.FullName ?? "Bilinmiyor")}," +
                        $"{transfer.TransferDate:dd.MM.yyyy HH:mm}," +
                        $"{EscapeCsvValue(transfer.PickupAddress)}," +
                        $"{EscapeCsvValue(transfer.DropoffAddress)}," +
                        $"{transfer.FinalPrice}," +
                        $"{transfer.Currency}," +
                        $"{EscapeCsvValue(transfer.Status)}");
                }

                var fileName = $"Transferler_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var content = Encoding.UTF8.GetBytes(csv.ToString());

                return new ExportResult
                {
                    FileContent = content,
                    FileName = fileName,
                    ContentType = "text/csv; charset=utf-8",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer listesi CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportJournalToCsvAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _journalEntryRepository.GetAll(x => !x.IsDeleted)
                    .Include(j => j.Lines)
                    .AsQueryable();

                if (startDate.HasValue)
                {
                    var start = startDate.Value.Date;
                    query = query.Where(j => j.PostingDate >= start);
                }

                if (endDate.HasValue)
                {
                    // inclusive end-of-day by using < next day
                    var endExclusive = endDate.Value.Date.AddDays(1);
                    query = query.Where(j => j.PostingDate < endExclusive);
                }

                var journals = await query
                    .OrderByDescending(j => j.PostingDate)
                    .ThenByDescending(j => j.Id)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("JournalEntryId,InvoiceId,PostingDate,Currency,Description,TotalDebit,TotalCredit,CreatedBy,LineAccountCode,LineDebit,LineCredit,LineDescription");

                foreach (var je in journals)
                {
                    var lines = je.Lines?.Any() == true ? je.Lines : new List<JournalLine> { new JournalLine() };

                    foreach (var line in lines)
                    {
                        csv.AppendLine(
                            $"{je.Id}," +
                            $"{(je.InvoiceId?.ToString() ?? string.Empty)}," +
                            $"{je.PostingDate:yyyy-MM-dd}," +
                            $"{EscapeCsvValue(je.Currency)}," +
                            $"{EscapeCsvValue(je.Description)}," +
                            $"{je.TotalDebit}," +
                            $"{je.TotalCredit}," +
                            $"{EscapeCsvValue(je.CreatedBy)}," +
                            $"{EscapeCsvValue(line.AccountCode)}," +
                            $"{line.Debit}," +
                            $"{line.Credit}," +
                            $"{EscapeCsvValue(line.Description)}"
                        );
                    }
                }

                var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                var range = $"{startDate?.ToString("yyyyMMdd") ?? "ALL"}_{endDate?.ToString("yyyyMMdd") ?? "ALL"}";
                var fileName = $"Journal_{range}_{stamp}.csv";
                var content = Encoding.UTF8.GetBytes(csv.ToString());

                return new ExportResult
                {
                    FileContent = content,
                    FileName = fileName,
                    ContentType = "text/csv; charset=utf-8",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Journal CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportJournalToExcelAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _journalEntryRepository.GetAll(x => !x.IsDeleted)
                    .Include(j => j.Lines)
                    .AsQueryable();

                if (startDate.HasValue)
                {
                    var start = startDate.Value.Date;
                    query = query.Where(j => j.PostingDate >= start);
                }

                if (endDate.HasValue)
                {
                    var endExclusive = endDate.Value.Date.AddDays(1);
                    query = query.Where(j => j.PostingDate < endExclusive);
                }

                var journals = await query
                    .OrderByDescending(j => j.PostingDate)
                    .ThenByDescending(j => j.Id)
                    .ToListAsync();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Journal");

                worksheet.Cell(1, 1).Value = "JournalEntryId";
                worksheet.Cell(1, 2).Value = "InvoiceId";
                worksheet.Cell(1, 3).Value = "PostingDate";
                worksheet.Cell(1, 4).Value = "Currency";
                worksheet.Cell(1, 5).Value = "Description";
                worksheet.Cell(1, 6).Value = "TotalDebit";
                worksheet.Cell(1, 7).Value = "TotalCredit";
                worksheet.Cell(1, 8).Value = "CreatedBy";
                worksheet.Cell(1, 9).Value = "LineAccountCode";
                worksheet.Cell(1, 10).Value = "LineDebit";
                worksheet.Cell(1, 11).Value = "LineCredit";
                worksheet.Cell(1, 12).Value = "LineDescription";

                var headerRange = worksheet.Range(1, 1, 1, 12);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 2;
                foreach (var je in journals)
                {
                    var lines = je.Lines?.Any() == true ? je.Lines : new List<JournalLine> { new JournalLine() };

                    foreach (var line in lines)
                    {
                        worksheet.Cell(row, 1).Value = je.Id;
                        worksheet.Cell(row, 2).Value = je.InvoiceId?.ToString() ?? "";
                        worksheet.Cell(row, 3).Value = je.PostingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        worksheet.Cell(row, 4).Value = je.Currency;
                        worksheet.Cell(row, 5).Value = je.Description;
                        worksheet.Cell(row, 6).Value = je.TotalDebit;
                        worksheet.Cell(row, 7).Value = je.TotalCredit;
                        worksheet.Cell(row, 8).Value = je.CreatedBy ?? "";
                        worksheet.Cell(row, 9).Value = line.AccountCode;
                        worksheet.Cell(row, 10).Value = line.Debit;
                        worksheet.Cell(row, 11).Value = line.Credit;
                        worksheet.Cell(row, 12).Value = line.Description ?? "";
                        row++;
                    }
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                var range = $"{startDate?.ToString("yyyyMMdd") ?? "ALL"}_{endDate?.ToString("yyyyMMdd") ?? "ALL"}";
                var fileName = $"Journal_{range}_{stamp}.xlsx";

                return new ExportResult
                {
                    FileContent = stream.ToArray(),
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Journal Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportGuestLedgerToCsvAsync(int? guestId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("Tarih,Tip,Referans,Açıklama,Debit (Borç),Credit (Alacak),Para Birimi,Durum");

                // Invoices
                var invoiceQuery = _invoiceRepository.GetAll(x => !x.IsDeleted)
                    .Include(i => i.Guest)
                    .AsQueryable();
                
                if (guestId.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.GuestId == guestId.Value);
                if (startDate.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.IssueDate >= startDate.Value);
                if (endDate.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.IssueDate <= endDate.Value);

                var invoices = await invoiceQuery.OrderBy(i => i.IssueDate).ToListAsync();
                foreach (var invoice in invoices)
                {
                    csv.AppendLine($"{invoice.IssueDate:yyyy-MM-dd},Fatura,{invoice.InvoiceNumber},{EscapeCsvValue(invoice.Notes)},,{invoice.TotalAmount:F2},{invoice.Currency},{invoice.Status}");
                }

                // Payments
                var paymentQuery = _paymentRepository.GetAll(x => !x.IsDeleted)
                    .Include(p => p.Guest)
                    .AsQueryable();
                
                if (guestId.HasValue)
                    paymentQuery = paymentQuery.Where(p => p.GuestId == guestId.Value);
                if (startDate.HasValue)
                    paymentQuery = paymentQuery.Where(p => p.PaymentDate >= startDate.Value);
                if (endDate.HasValue)
                    paymentQuery = paymentQuery.Where(p => p.PaymentDate <= endDate.Value);

                var payments = await paymentQuery.OrderBy(p => p.PaymentDate).ToListAsync();
                foreach (var payment in payments)
                {
                    var amount = payment.Status == PaymentStatus.Refunded ? -payment.Amount : payment.Amount;
                    csv.AppendLine($"{payment.PaymentDate:yyyy-MM-dd},Ödeme,{payment.PaymentNumber},{EscapeCsvValue(payment.Notes ?? "Ödeme")},{amount:F2},,{payment.Currency},{payment.Status}");
                }

                var filename = $"guest_ledger_{guestId ?? 0}_{DateTime.UtcNow:yyyyMMdd}.csv";
                return new ExportResult
                {
                    IsSuccess = true,
                    FileContent = Encoding.UTF8.GetBytes(csv.ToString()),
                    FileName = filename,
                    ContentType = "text/csv; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Guest Ledger CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportGuestLedgerToExcelAsync(int? guestId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Guest Ledger");

                // Headers
                worksheet.Cell(1, 1).Value = "Tarih";
                worksheet.Cell(1, 2).Value = "Tip";
                worksheet.Cell(1, 3).Value = "Referans";
                worksheet.Cell(1, 4).Value = "Açıklama";
                worksheet.Cell(1, 5).Value = "Debit (Borç)";
                worksheet.Cell(1, 6).Value = "Credit (Alacak)";
                worksheet.Cell(1, 7).Value = "Para Birimi";
                worksheet.Cell(1, 8).Value = "Durum";

                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;

                // Invoices
                var invoiceQuery = _invoiceRepository.GetAll(x => !x.IsDeleted)
                    .Include(i => i.Guest)
                    .AsQueryable();
                
                if (guestId.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.GuestId == guestId.Value);
                if (startDate.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.IssueDate >= startDate.Value);
                if (endDate.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.IssueDate <= endDate.Value);

                var invoices = await invoiceQuery.OrderBy(i => i.IssueDate).ToListAsync();
                foreach (var invoice in invoices)
                {
                    worksheet.Cell(row, 1).Value = invoice.IssueDate;
                    worksheet.Cell(row, 2).Value = "Fatura";
                    worksheet.Cell(row, 3).Value = invoice.InvoiceNumber;
                    worksheet.Cell(row, 4).Value = invoice.Notes ?? string.Empty;
                    worksheet.Cell(row, 6).Value = invoice.TotalAmount;
                    worksheet.Cell(row, 7).Value = invoice.Currency;
                    worksheet.Cell(row, 8).Value = invoice.Status.ToString();
                    row++;
                }

                // Payments
                var paymentQuery = _paymentRepository.GetAll(x => !x.IsDeleted)
                    .Include(p => p.Guest)
                    .AsQueryable();
                
                if (guestId.HasValue)
                    paymentQuery = paymentQuery.Where(p => p.GuestId == guestId.Value);
                if (startDate.HasValue)
                    paymentQuery = paymentQuery.Where(p => p.PaymentDate >= startDate.Value);
                if (endDate.HasValue)
                    paymentQuery = paymentQuery.Where(p => p.PaymentDate <= endDate.Value);

                var payments = await paymentQuery.OrderBy(p => p.PaymentDate).ToListAsync();
                foreach (var payment in payments)
                {
                    var amount = payment.Status == PaymentStatus.Refunded ? -payment.Amount : payment.Amount;
                    worksheet.Cell(row, 1).Value = payment.PaymentDate;
                    worksheet.Cell(row, 2).Value = "Ödeme";
                    worksheet.Cell(row, 3).Value = payment.PaymentNumber;
                    worksheet.Cell(row, 4).Value = payment.Notes ?? "Ödeme";
                    worksheet.Cell(row, 5).Value = amount;
                    worksheet.Cell(row, 7).Value = payment.Currency;
                    worksheet.Cell(row, 8).Value = payment.Status.ToString();
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                var filename = $"guest_ledger_{guestId ?? 0}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                
                return new ExportResult
                {
                    IsSuccess = true,
                    FileContent = stream.ToArray(),
                    FileName = filename,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Guest Ledger Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportSupplierLedgerToCsvAsync(int? supplierId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("Tarih,Tip,Referans,Açıklama,Maliyet,Para Birimi,Durum");

                var costQuery = _supplierCostRepository.GetAll(x => !x.IsDeleted)
                    .Include(c => c.Supplier)
                    .AsQueryable();
                
                if (supplierId.HasValue)
                    costQuery = costQuery.Where(c => c.SupplierId == supplierId.Value);
                if (startDate.HasValue)
                    costQuery = costQuery.Where(c => c.CreatedDate >= startDate.Value);
                if (endDate.HasValue)
                    costQuery = costQuery.Where(c => c.CreatedDate <= endDate.Value);

                var costs = await costQuery.OrderBy(c => c.CreatedDate).ToListAsync();
                foreach (var cost in costs)
                {
                    var serviceType = cost.TransferId.HasValue ? "Transfer" :
                                     cost.CityTourId.HasValue ? "CityTour" :
                                     cost.YachtTourId.HasValue ? "YachtTour" : "Diğer";
                    var reference = cost.TransferId?.ToString() ?? cost.CityTourId?.ToString() ?? cost.YachtTourId?.ToString() ?? "";
                    csv.AppendLine($"{cost.CreatedDate:yyyy-MM-dd},{serviceType},{reference},{EscapeCsvValue(cost.Description ?? cost.CostType)},{cost.CostAmount:F2},{cost.Currency},{(cost.IsActive ? "Aktif" : "Pasif")}");
                }

                var filename = $"supplier_ledger_{supplierId ?? 0}_{DateTime.UtcNow:yyyyMMdd}.csv";
                return new ExportResult
                {
                    IsSuccess = true,
                    FileContent = Encoding.UTF8.GetBytes(csv.ToString()),
                    FileName = filename,
                    ContentType = "text/csv; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Supplier Ledger CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportSupplierLedgerToExcelAsync(int? supplierId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Supplier Ledger");

                worksheet.Cell(1, 1).Value = "Tarih";
                worksheet.Cell(1, 2).Value = "Tip";
                worksheet.Cell(1, 3).Value = "Referans";
                worksheet.Cell(1, 4).Value = "Açıklama";
                worksheet.Cell(1, 5).Value = "Maliyet";
                worksheet.Cell(1, 6).Value = "Para Birimi";
                worksheet.Cell(1, 7).Value = "Durum";

                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                var costQuery = _supplierCostRepository.GetAll(x => !x.IsDeleted)
                    .Include(c => c.Supplier)
                    .AsQueryable();
                
                if (supplierId.HasValue)
                    costQuery = costQuery.Where(c => c.SupplierId == supplierId.Value);
                if (startDate.HasValue)
                    costQuery = costQuery.Where(c => c.CreatedDate >= startDate.Value);
                if (endDate.HasValue)
                    costQuery = costQuery.Where(c => c.CreatedDate <= endDate.Value);

                var costs = await costQuery.OrderBy(c => c.CreatedDate).ToListAsync();
                int row = 2;
                foreach (var cost in costs)
                {
                    var serviceType = cost.TransferId.HasValue ? "Transfer" :
                                     cost.CityTourId.HasValue ? "CityTour" :
                                     cost.YachtTourId.HasValue ? "YachtTour" : "Diğer";
                    var reference = cost.TransferId?.ToString() ?? cost.CityTourId?.ToString() ?? cost.YachtTourId?.ToString() ?? "";
                    worksheet.Cell(row, 1).Value = cost.CreatedDate;
                    worksheet.Cell(row, 2).Value = serviceType;
                    worksheet.Cell(row, 3).Value = reference;
                    worksheet.Cell(row, 4).Value = cost.Description ?? cost.CostType;
                    worksheet.Cell(row, 5).Value = cost.CostAmount;
                    worksheet.Cell(row, 6).Value = cost.Currency;
                    worksheet.Cell(row, 7).Value = cost.IsActive ? "Aktif" : "Pasif";
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                var filename = $"supplier_ledger_{supplierId ?? 0}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                
                return new ExportResult
                {
                    IsSuccess = true,
                    FileContent = stream.ToArray(),
                    FileName = filename,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Supplier Ledger Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportRoomLedgerToCsvAsync(string? roomNumber = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("Tarih,Tip,Misafir,Referans,Açıklama,Tutar,Para Birimi");

                // Room Assignments
                var assignmentQuery = _roomAssignmentRepository.GetAll(x => !x.IsDeleted)
                    .Include(a => a.Guest)
                    .AsQueryable();
                
                if (!string.IsNullOrEmpty(roomNumber))
                    assignmentQuery = assignmentQuery.Where(a => a.RoomNumber == roomNumber);
                if (startDate.HasValue)
                    assignmentQuery = assignmentQuery.Where(a => a.StartDate >= startDate.Value);
                if (endDate.HasValue)
                    assignmentQuery = assignmentQuery.Where(a => a.StartDate <= endDate.Value);

                var assignments = await assignmentQuery.OrderBy(a => a.StartDate).ToListAsync();
                foreach (var assignment in assignments)
                {
                    csv.AppendLine($"{assignment.StartDate:yyyy-MM-dd},Oda Ataması,{EscapeCsvValue(assignment.Guest.FullName)},{assignment.RoomNumber},{EscapeCsvValue(assignment.Notes ?? "Oda ataması")},,");
                }

                // Invoices for guests in this room
                var invoiceQuery = _invoiceRepository.GetAll(x => !x.IsDeleted)
                    .Include(i => i.Guest)
                    .AsQueryable();
                
                if (!string.IsNullOrEmpty(roomNumber))
                {
                    var guestIds = await assignmentQuery.Select(a => a.GuestId).Distinct().ToListAsync();
                    invoiceQuery = invoiceQuery.Where(i => guestIds.Contains(i.GuestId));
                }
                if (startDate.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.IssueDate >= startDate.Value);
                if (endDate.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.IssueDate <= endDate.Value);

                var invoices = await invoiceQuery.OrderBy(i => i.IssueDate).ToListAsync();
                foreach (var invoice in invoices)
                {
                    csv.AppendLine($"{invoice.IssueDate:yyyy-MM-dd},Fatura,{EscapeCsvValue(invoice.Guest.FullName)},{invoice.InvoiceNumber},{EscapeCsvValue(invoice.Notes)},{invoice.TotalAmount:F2},{invoice.Currency}");
                }

                var filename = $"room_ledger_{roomNumber ?? "all"}_{DateTime.UtcNow:yyyyMMdd}.csv";
                return new ExportResult
                {
                    IsSuccess = true,
                    FileContent = Encoding.UTF8.GetBytes(csv.ToString()),
                    FileName = filename,
                    ContentType = "text/csv; charset=utf-8"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Room Ledger CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportRoomLedgerToExcelAsync(string? roomNumber = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Room Ledger");

                worksheet.Cell(1, 1).Value = "Tarih";
                worksheet.Cell(1, 2).Value = "Tip";
                worksheet.Cell(1, 3).Value = "Misafir";
                worksheet.Cell(1, 4).Value = "Referans";
                worksheet.Cell(1, 5).Value = "Açıklama";
                worksheet.Cell(1, 6).Value = "Tutar";
                worksheet.Cell(1, 7).Value = "Para Birimi";

                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;

                // Room Assignments
                var assignmentQuery = _roomAssignmentRepository.GetAll(x => !x.IsDeleted)
                    .Include(a => a.Guest)
                    .AsQueryable();
                
                if (!string.IsNullOrEmpty(roomNumber))
                    assignmentQuery = assignmentQuery.Where(a => a.RoomNumber == roomNumber);
                if (startDate.HasValue)
                    assignmentQuery = assignmentQuery.Where(a => a.StartDate >= startDate.Value);
                if (endDate.HasValue)
                    assignmentQuery = assignmentQuery.Where(a => a.StartDate <= endDate.Value);

                var assignments = await assignmentQuery.OrderBy(a => a.StartDate).ToListAsync();
                foreach (var assignment in assignments)
                {
                    worksheet.Cell(row, 1).Value = assignment.StartDate;
                    worksheet.Cell(row, 2).Value = "Oda Ataması";
                    worksheet.Cell(row, 3).Value = assignment.Guest.FullName;
                    worksheet.Cell(row, 4).Value = assignment.RoomNumber;
                    worksheet.Cell(row, 5).Value = assignment.Notes ?? "Oda ataması";
                    row++;
                }

                // Invoices
                var invoiceQuery = _invoiceRepository.GetAll(x => !x.IsDeleted)
                    .Include(i => i.Guest)
                    .AsQueryable();
                
                if (!string.IsNullOrEmpty(roomNumber))
                {
                    var guestIds = await assignmentQuery.Select(a => a.GuestId).Distinct().ToListAsync();
                    invoiceQuery = invoiceQuery.Where(i => guestIds.Contains(i.GuestId));
                }
                if (startDate.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.IssueDate >= startDate.Value);
                if (endDate.HasValue)
                    invoiceQuery = invoiceQuery.Where(i => i.IssueDate <= endDate.Value);

                var invoices = await invoiceQuery.OrderBy(i => i.IssueDate).ToListAsync();
                foreach (var invoice in invoices)
                {
                    worksheet.Cell(row, 1).Value = invoice.IssueDate;
                    worksheet.Cell(row, 2).Value = "Fatura";
                    worksheet.Cell(row, 3).Value = invoice.Guest.FullName;
                    worksheet.Cell(row, 4).Value = invoice.InvoiceNumber;
                    worksheet.Cell(row, 5).Value = invoice.Notes;
                    worksheet.Cell(row, 6).Value = invoice.TotalAmount;
                    worksheet.Cell(row, 7).Value = invoice.Currency;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                var filename = $"room_ledger_{roomNumber ?? "all"}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                
                return new ExportResult
                {
                    IsSuccess = true,
                    FileContent = stream.ToArray(),
                    FileName = filename,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Room Ledger Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        #region Private Methods

        /// <summary>
        /// CSV değerini escape eder (virgül, tırnak, yeni satır karakterleri için)
        /// </summary>
        private string EscapeCsvValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Virgül, tırnak veya yeni satır içeriyorsa tırnak içine al
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        #endregion

        #region VAT Reports Export

        public async Task<ExportResult> ExportVatAccrualReportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null, string? currency = null)
        {
            try
            {
                var vatReport = await _reportsService.GetVatAccrualReportAsync(startDate, endDate, currency);

                using var workbook = new XLWorkbook();

                // Özet sayfası
                var summarySheet = workbook.Worksheets.Add("KDV Tahakkuk Özeti");
                summarySheet.Cell(1, 1).Value = "KDV Tahakkuk Raporu";
                summarySheet.Cell(2, 1).Value = "Başlangıç Tarihi";
                summarySheet.Cell(2, 2).Value = startDate?.ToString("dd.MM.yyyy") ?? "Tümü";
                summarySheet.Cell(3, 1).Value = "Bitiş Tarihi";
                summarySheet.Cell(3, 2).Value = endDate?.ToString("dd.MM.yyyy") ?? "Tümü";
                summarySheet.Cell(4, 1).Value = "Para Birimi";
                summarySheet.Cell(4, 2).Value = currency ?? "Tümü";
                summarySheet.Cell(5, 1).Value = "Toplam Fatura Sayısı";
                summarySheet.Cell(5, 2).Value = vatReport.TotalInvoiceCount;
                summarySheet.Cell(6, 1).Value = "Post Edilmiş Journal Sayısı";
                summarySheet.Cell(6, 2).Value = vatReport.PostedJournalCount;

                // Currency bazlı toplam VAT
                int row = 8;
                summarySheet.Cell(row, 1).Value = "Para Birimi";
                summarySheet.Cell(row, 2).Value = "Toplam KDV";
                row++;
                foreach (var kvp in vatReport.TotalVatByCurrency)
                {
                    summarySheet.Cell(row, 1).Value = kvp.Key;
                    summarySheet.Cell(row, 2).Value = kvp.Value;
                    row++;
                }

                // Post edilmemiş VAT
                row += 2;
                summarySheet.Cell(row, 1).Value = "Post Edilmemiş KDV";
                row++;
                summarySheet.Cell(row, 1).Value = "Para Birimi";
                summarySheet.Cell(row, 2).Value = "Tutar";
                row++;
                foreach (var kvp in vatReport.UnpostedVatByCurrency)
                {
                    summarySheet.Cell(row, 1).Value = kvp.Key;
                    summarySheet.Cell(row, 2).Value = kvp.Value;
                    row++;
                }

                // Servis tipine göre VAT
                var serviceTypeSheet = workbook.Worksheets.Add("Servis Tipine Göre KDV");
                serviceTypeSheet.Cell(1, 1).Value = "Servis Tipi";
                serviceTypeSheet.Cell(1, 2).Value = "Para Birimi";
                serviceTypeSheet.Cell(1, 3).Value = "KDV Tutarı";

                var headerRange = serviceTypeSheet.Range(1, 1, 1, 3);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row = 2;
                foreach (var serviceType in vatReport.VatByServiceType)
                {
                    foreach (var currencyKvp in serviceType.Value)
                    {
                        serviceTypeSheet.Cell(row, 1).Value = serviceType.Key;
                        serviceTypeSheet.Cell(row, 2).Value = currencyKvp.Key;
                        serviceTypeSheet.Cell(row, 3).Value = currencyKvp.Value;
                        row++;
                    }
                }

                summarySheet.Columns().AdjustToContents();
                serviceTypeSheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"KDV_Tahakkuk_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return new ExportResult
                {
                    FileContent = stream.ToArray(),
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"KDV tahakkuk raporu Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportVatAccrualReportToCsvAsync(DateTime? startDate = null, DateTime? endDate = null, string? currency = null)
        {
            try
            {
                var vatReport = await _reportsService.GetVatAccrualReportAsync(startDate, endDate, currency);

                var csv = new StringBuilder();
                csv.AppendLine("KDV Tahakkuk Raporu");
                csv.AppendLine($"Başlangıç Tarihi,{startDate?.ToString("dd.MM.yyyy") ?? "Tümü"}");
                csv.AppendLine($"Bitiş Tarihi,{endDate?.ToString("dd.MM.yyyy") ?? "Tümü"}");
                csv.AppendLine($"Para Birimi,{currency ?? "Tümü"}");
                csv.AppendLine($"Toplam Fatura Sayısı,{vatReport.TotalInvoiceCount}");
                csv.AppendLine($"Post Edilmiş Journal Sayısı,{vatReport.PostedJournalCount}");
                csv.AppendLine();
                csv.AppendLine("Para Birimi,Toplam KDV");
                foreach (var kvp in vatReport.TotalVatByCurrency)
                {
                    csv.AppendLine($"{kvp.Key},{kvp.Value}");
                }
                csv.AppendLine();
                csv.AppendLine("Post Edilmemiş KDV");
                csv.AppendLine("Para Birimi,Tutar");
                foreach (var kvp in vatReport.UnpostedVatByCurrency)
                {
                    csv.AppendLine($"{kvp.Key},{kvp.Value}");
                }
                csv.AppendLine();
                csv.AppendLine("Servis Tipine Göre KDV");
                csv.AppendLine("Servis Tipi,Para Birimi,KDV Tutarı");
                foreach (var serviceType in vatReport.VatByServiceType)
                {
                    foreach (var currencyKvp in serviceType.Value)
                    {
                        csv.AppendLine($"{EscapeCsvValue(serviceType.Key)},{currencyKvp.Key},{currencyKvp.Value}");
                    }
                }

                var fileName = $"KDV_Tahakkuk_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var content = Encoding.UTF8.GetBytes(csv.ToString());

                return new ExportResult
                {
                    FileContent = content,
                    FileName = fileName,
                    ContentType = "text/csv; charset=utf-8",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"KDV tahakkuk raporu CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportVatPeriodReportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null, string? periodType = null, string? currency = null)
        {
            try
            {
                var periodReports = await _reportsService.GetVatPeriodReportAsync(startDate, endDate, periodType, currency);

                using var workbook = new XLWorkbook();
                var sheet = workbook.Worksheets.Add("Dönem Bazlı KDV Raporu");

                // Başlıklar
                sheet.Cell(1, 1).Value = "Dönem Başlangıç";
                sheet.Cell(1, 2).Value = "Dönem Bitiş";
                sheet.Cell(1, 3).Value = "Para Birimi";
                sheet.Cell(1, 4).Value = "Toplam KDV";
                sheet.Cell(1, 5).Value = "Toplam Net Tutar";
                sheet.Cell(1, 6).Value = "Toplam Brüt Tutar";
                sheet.Cell(1, 7).Value = "Fatura Sayısı";

                var headerRange = sheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Veriler
                int row = 2;
                foreach (var report in periodReports)
                {
                    sheet.Cell(row, 1).Value = report.PeriodStart.ToString("dd.MM.yyyy");
                    sheet.Cell(row, 2).Value = report.PeriodEnd.ToString("dd.MM.yyyy");
                    sheet.Cell(row, 3).Value = report.Currency;
                    sheet.Cell(row, 4).Value = report.TotalVat;
                    sheet.Cell(row, 5).Value = report.TotalNetAmount;
                    sheet.Cell(row, 6).Value = report.TotalGrossAmount;
                    sheet.Cell(row, 7).Value = report.InvoiceCount;
                    row++;
                }

                // Servis tipine göre KDV detayları (ikinci sayfa)
                var serviceTypeSheet = workbook.Worksheets.Add("Servis Tipine Göre KDV");
                serviceTypeSheet.Cell(1, 1).Value = "Dönem";
                serviceTypeSheet.Cell(1, 2).Value = "Servis Tipi";
                serviceTypeSheet.Cell(1, 3).Value = "KDV Tutarı";

                var serviceHeaderRange = serviceTypeSheet.Range(1, 1, 1, 3);
                serviceHeaderRange.Style.Font.Bold = true;
                serviceHeaderRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                serviceHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row = 2;
                foreach (var report in periodReports)
                {
                    var periodLabel = $"{report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
                    foreach (var serviceType in report.VatByServiceType)
                    {
                        serviceTypeSheet.Cell(row, 1).Value = periodLabel;
                        serviceTypeSheet.Cell(row, 2).Value = serviceType.Key;
                        serviceTypeSheet.Cell(row, 3).Value = serviceType.Value;
                        row++;
                    }
                }

                sheet.Columns().AdjustToContents();
                serviceTypeSheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Dönem_Bazlı_KDV_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return new ExportResult
                {
                    FileContent = stream.ToArray(),
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dönem bazlı KDV raporu Excel'e aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Excel'e aktarılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ExportResult> ExportVatPeriodReportToCsvAsync(DateTime? startDate = null, DateTime? endDate = null, string? periodType = null, string? currency = null)
        {
            try
            {
                var periodReports = await _reportsService.GetVatPeriodReportAsync(startDate, endDate, periodType, currency);

                var csv = new StringBuilder();
                csv.AppendLine("Dönem Bazlı KDV Raporu");
                csv.AppendLine($"Başlangıç Tarihi,{startDate?.ToString("dd.MM.yyyy") ?? "Tümü"}");
                csv.AppendLine($"Bitiş Tarihi,{endDate?.ToString("dd.MM.yyyy") ?? "Tümü"}");
                csv.AppendLine($"Dönem Tipi,{periodType ?? "Otomatik"}");
                csv.AppendLine($"Para Birimi,{currency ?? "Tümü"}");
                csv.AppendLine();
                csv.AppendLine("Dönem Başlangıç,Dönem Bitiş,Para Birimi,Toplam KDV,Toplam Net Tutar,Toplam Brüt Tutar,Fatura Sayısı");
                foreach (var report in periodReports)
                {
                    csv.AppendLine($"{report.PeriodStart:dd.MM.yyyy}," +
                        $"{report.PeriodEnd:dd.MM.yyyy}," +
                        $"{report.Currency}," +
                        $"{report.TotalVat}," +
                        $"{report.TotalNetAmount}," +
                        $"{report.TotalGrossAmount}," +
                        $"{report.InvoiceCount}");
                }
                csv.AppendLine();
                csv.AppendLine("Servis Tipine Göre KDV");
                csv.AppendLine("Dönem,Servis Tipi,KDV Tutarı");
                foreach (var report in periodReports)
                {
                    var periodLabel = $"{report.PeriodStart:dd.MM.yyyy} - {report.PeriodEnd:dd.MM.yyyy}";
                    foreach (var serviceType in report.VatByServiceType)
                    {
                        csv.AppendLine($"{periodLabel},{EscapeCsvValue(serviceType.Key)},{serviceType.Value}");
                    }
                }

                var fileName = $"Dönem_Bazlı_KDV_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var content = Encoding.UTF8.GetBytes(csv.ToString());

                return new ExportResult
                {
                    FileContent = content,
                    FileName = fileName,
                    ContentType = "text/csv; charset=utf-8",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dönem bazlı KDV raporu CSV'ye aktarılırken hata: {ex.Message}");
                return new ExportResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"CSV'ye aktarılırken hata: {ex.Message}"
                };
            }
        }

        #endregion
    }
}

