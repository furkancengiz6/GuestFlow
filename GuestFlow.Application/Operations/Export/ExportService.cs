using ClosedXML.Excel;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Reports;
using GuestFlow.Application.Operations.Transfer;
using GuestFlow.Domain.Entities.Core;
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
        private readonly IReportsService _reportsService;
        private readonly ILogger<ExportService> _logger;

        public ExportService(
            IRepository<GuestEntity> guestRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<DailyRevenueEntity> dailyRevenueRepository,
            IRepository<JournalEntry> journalEntryRepository,
            IReportsService reportsService,
            ILogger<ExportService> logger)
        {
            _guestRepository = guestRepository;
            _invoiceRepository = invoiceRepository;
            _transferRepository = transferRepository;
            _dailyRevenueRepository = dailyRevenueRepository;
            _journalEntryRepository = journalEntryRepository;
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
                summarySheet.Cell(1, 2).Value = revenueSummary.TotalRevenue;
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
                csv.AppendLine($"Toplam Gelir,{revenueSummary.TotalRevenue}");
                csv.AppendLine($"Para Birimi,TRY");
                csv.AppendLine($"Başlangıç Tarihi,{startDate?.ToString("dd.MM.yyyy") ?? "Tümü"}");
                csv.AppendLine($"Bitiş Tarihi,{endDate?.ToString("dd.MM.yyyy") ?? "Tümü"}");
                csv.AppendLine();
                csv.AppendLine("Tarih,Gelir,Para Birimi");

                foreach (var daily in dailyRevenues)
                {
                    csv.AppendLine($"{daily.Date:dd.MM.yyyy},{daily.TotalRevenue},TRY");
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
    }
}

