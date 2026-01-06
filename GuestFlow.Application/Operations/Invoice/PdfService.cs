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

