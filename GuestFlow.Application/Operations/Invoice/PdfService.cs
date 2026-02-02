using GuestFlow.Domain.Entities.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Invoice
{
    public class PdfService : IPdfService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PdfService> _logger;
        private readonly IPdfUrlService _pdfUrlService;
        private readonly string _pdfStoragePath;

        public PdfService(IConfiguration configuration, ILogger<PdfService> logger, IPdfUrlService pdfUrlService)
        {
            _configuration = configuration;
            _logger = logger;
            _pdfUrlService = pdfUrlService;
            _pdfStoragePath = _configuration["PdfSettings:StoragePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");

            // PDF klasörünü oluştur
            if (!Directory.Exists(_pdfStoragePath))
            {
                Directory.CreateDirectory(_pdfStoragePath);
            }

            // QuestPDF lisansını ayarla (community edition için)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateInvoicePdfAsync(InvoicesEntity invoice, GuestEntity guest, PersonnelEntity? personnel = null)
        {
            return await Task.Run(() => GenerateInvoicePdf(invoice, guest, personnel));
        }

        private string GenerateInvoicePdf(InvoicesEntity invoice, GuestEntity guest, PersonnelEntity? personnel = null)
        {
            try
            {
                // PDF dosya adını oluştur (IPdfUrlService kullanarak)
                var fileName = _pdfUrlService.GenerateFileName(invoice.InvoiceNumber);
                var filePath = Path.Combine(_pdfStoragePath, fileName);

                // Invoice type - generic for multi-service
                string invoiceType = "Fatura";

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .PaddingBottom(1, Unit.Centimetre)
                            .Text(invoiceType)
                            .SemiBold().FontSize(16).AlignCenter();

                        page.Content()
                            .Column(column =>
                            {
                                // Fatura Bilgileri
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("FATURA BİLGİLERİ").FontSize(12).Bold();
                                        col.Item().Text($"Fatura No: {invoice.InvoiceNumber}");
                                        col.Item().Text($"Tarih: {invoice.IssueDate:dd.MM.yyyy}");
                                        col.Item().Text($"Para Birimi: {invoice.Currency}");
                                        col.Item().Text($"Durum: {invoice.Status.ToString()}");
                                        if (invoice.IsPdfGenerated)
                                        {
                                            col.Item().Text($"PDF Oluşturulma: {invoice.PdfGeneratedDate:dd.MM.yyyy HH:mm}");
                                        }
                                    });

                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("MİSAFİR BİLGİLERİ").FontSize(12).Bold();
                                        col.Item().Text($"Ad Soyad: {guest.FullName}");
                                        col.Item().Text($"Misafir Kodu: {guest.GuestCode}");
                                        if (!string.IsNullOrEmpty(guest.Email))
                                            col.Item().Text($"E-posta: {guest.Email}");
                                        if (!string.IsNullOrEmpty(guest.PhoneNumber))
                                            col.Item().Text($"Telefon: {guest.PhoneNumber}");
                                        col.Item().Text($"Uyruk: {guest.Nationality}");
                                    });
                                });

                                // Hizmet Detayları - List all invoice items
                                column.Item().PaddingTop(15).Text("HİZMET DETAYLARI").FontSize(12).Bold();

                                if (invoice.InvoiceItems != null && invoice.InvoiceItems.Any())
                                {
                                    foreach (var item in invoice.InvoiceItems)
                                    {
                                        column.Item().PaddingTop(5).Text($"{item.ServiceType}: {item.Notes ?? "Hizmet"}").FontSize(10).Bold();
                                        column.Item().Text($"Tarih: {item.CreatedDate:dd.MM.yyyy}").FontSize(9);
                                        column.Item().Text($"Tutar: {item.Amount:N2} {item.Currency}").FontSize(9);
                                    }
                                }

                                // Fiyat Detayları
                                column.Item().PaddingTop(15).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Hizmet Açıklaması").Bold();
                                        header.Cell().Element(CellStyle).AlignRight().Text("Miktar").Bold();
                                        header.Cell().Element(CellStyle).AlignRight().Text("Tutar").Bold();
                                    });

                                    // List each invoice item
                                    if (invoice.InvoiceItems != null)
                                    {
                                        foreach (var item in invoice.InvoiceItems)
                                        {
                                            table.Cell().Element(CellStyle).Text($"{item.ServiceType}: {item.Notes ?? "Hizmet"}");
                                            table.Cell().Element(CellStyle).AlignRight().Text("1");
                                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Amount:N2} {item.Currency}");
                                        }
                                    }

                                    // Toplam
                                    table.Cell().Element(CellStyle).Text("TOPLAM").Bold().FontSize(11);
                                    table.Cell().Element(CellStyle).AlignRight().Text("");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"{invoice.TotalAmount:N2} {invoice.Currency}").Bold().FontSize(11);
                                });

                                // Notlar
                                if (!string.IsNullOrEmpty(invoice.Notes))
                                {
                                    column.Item().PaddingTop(20).Text("FATURA NOTLARI").FontSize(11).Bold();
                                    column.Item().Text(invoice.Notes).FontSize(10);
                                }

                                // Personel bilgisi (varsa)
                                if (personnel != null)
                                {
                                    column.Item().PaddingTop(20).Text("PERSONEL BİLGİLERİ").FontSize(11).Bold();
                                    column.Item().Text($"Ad Soyad: {personnel.FullName}");
                                    if (!string.IsNullOrEmpty(personnel.Email))
                                        column.Item().Text($"E-posta: {personnel.Email}");
                                }

                                // Immutability notice
                                if (invoice.IsPdfGenerated)
                                {
                                    column.Item().PaddingTop(20).Text("BU FATURA PDF OLARAK OLUŞTURULDUĞU İÇİN DEĞIŞTİRİLEMEZ").FontSize(8).Bold().AlignCenter();
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Sayfa ").FontSize(8);
                                x.CurrentPageNumber().FontSize(8);
                                x.Span(" / ").FontSize(8);
                                x.TotalPages().FontSize(8);
                            });
                    });
                });

                // PDF'i oluştur ve kaydet
                document.GeneratePdf(filePath);

                _logger.LogInformation($"PDF fatura oluşturuldu: {filePath}");

                // Relative URL döndür (IPdfUrlService kullanarak)
                var relativeUrl = _pdfUrlService.CreateUrlFromFileName(fileName);
                return relativeUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PDF oluşturulurken hata: {ex.Message}");
                throw new Exception($"PDF oluşturulurken hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateWeeklyReportPdfAsync(object reportData)
        {
            return await Task.Run(() => GenerateWeeklyReportPdf(reportData));
        }

        private string GenerateWeeklyReportPdf(object reportData)
        {
            try
            {
                var fileName = $"weekly_report_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
                var filePath = Path.Combine(_pdfStoragePath, fileName);

                // For now, casting to dynamic to access properties easily
                // In production, use a specific DTO
                dynamic data = reportData;

                // Cast to local typed variables to avoid dynamic dispatch issues with QuestPDF extension methods
                DateTime sDate = data.StartDate;
                DateTime eDate = data.EndDate;
                var revenueByCategory = data.RevenueByCategory as IDictionary<string, decimal>;
                int totalBookings = data.TotalBookings;
                int transferCount = data.TransferCount;
                int cityTourCount = data.CityTourCount;
                int yachtTourCount = data.YachtTourCount;
                int newGuestCount = data.NewGuestCount;
                int vIPGuestCount = data.VIPGuestCount;
                double averageSatisfaction = data.AverageSatisfaction;
                var popularDestinations = data.PopularDestinations as IEnumerable<dynamic>;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .PaddingBottom(1, Unit.Centimetre)
                            .Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("GUESTFLOW OPERASYONEL RAPOR").SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
                                    col.Item().Text("Haftalık Özet ve Analiz").FontSize(12).Italic();
                                });

                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                    col.Item().Text($"Rapor Tarihi: {DateTime.UtcNow:dd.MM.yyyy HH:mm}");
                                    col.Item().Text($"Dönem: {sDate:dd.MM.yyyy} - {eDate:dd.MM.yyyy}");
                                });
                            });

                        page.Content()
                            .Column(column =>
                            {
                                // Gelir Özeti Bölümü
                                column.Item().PaddingTop(10).Text("GELİR ÖZETİ").FontSize(14).Bold().Underline();
                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Hizmet Türü").Bold();
                                        header.Cell().Element(CellStyle).AlignRight().Text("Toplam Gelir").Bold();
                                    });

                                    // Safely check and add rows
                                    if (revenueByCategory != null)
                                    {
                                        foreach (var entry in revenueByCategory)
                                        {
                                            table.Cell().Element(CellStyle).Text(entry.Key);
                                            table.Cell().Element(CellStyle).AlignRight().Text($"{entry.Value:N2} USD");
                                        }
                                    }
                                });

                                // Operasyonel İstatistikler Bölümü
                                column.Item().PaddingTop(20).Text("OPERASYONEL İSTATİSTİKLER").FontSize(14).Bold().Underline();
                                column.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("Rezervasyon Sayıları").SemiBold();
                                        col.Item().Text($"- Toplam Rezervasyon: {totalBookings}");
                                        col.Item().Text($"- Transfer: {transferCount}");
                                        col.Item().Text($"- Şehir Turu: {cityTourCount}");
                                        col.Item().Text($"- Yat Turu: {yachtTourCount}");
                                    });

                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("Misafir Verileri").SemiBold();
                                        col.Item().Text($"- Yeni Misafirler: {newGuestCount}");
                                        col.Item().Text($"- VIP Misafir Sayısı: {vIPGuestCount}");
                                        col.Item().Text($"- Ortalama Memnuniyet: {averageSatisfaction:N1}/5");
                                    });
                                });

                                // Popüler Destinasyonlar (Varsa)
                                if (popularDestinations != null && popularDestinations.Any())
                                {
                                    column.Item().PaddingTop(20).Text("POPÜLER DESTİNASYONLAR").FontSize(14).Bold().Underline();
                                    foreach (var dest in popularDestinations)
                                    {
                                        column.Item().Text($"- {dest.CityName}: {dest.BookingCount} rezervasyon");
                                    }
                                }

                                // Notlar
                                column.Item().PaddingTop(30).Text("NOTLAR").FontSize(12).Bold();
                                column.Item().Text("Bu rapor sistem tarafından otomatik oluşturulmuştur. Finansal veriler tahsilat bazlıdır.").FontSize(9).Italic();
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Sayfa ").FontSize(8);
                                x.CurrentPageNumber().FontSize(8);
                                x.Span(" / ").FontSize(8);
                                x.TotalPages().FontSize(8);
                            });
                    });
                });

                document.GeneratePdf(filePath);
                _logger.LogInformation($"Haftalık operasyonel rapor oluşturuldu: {filePath}");

                var relativeUrl = _pdfUrlService.CreateUrlFromFileName(fileName);
                return relativeUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Haftalık rapor PDF oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5)
                .PaddingHorizontal(10);
        }
    }
}

