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

                // Invoice tipini ve detaylarını belirle
                string invoiceType = "Fatura";
                string serviceDescription = invoice.Notes ?? "Hizmet";
                string? serviceDetails = null;
                DateTime? serviceDate = null;
                decimal? discountPercentage = null;
                decimal? originalPrice = null;

                if (invoice.TransferId.HasValue && invoice.Transfer != null)
                {
                    invoiceType = "Transfer Faturası";
                    serviceDescription = "Transfer Hizmeti";
                    serviceDate = invoice.Transfer.TransferDate;
                    serviceDetails = $"Alış: {invoice.Transfer.PickupAddress}\nBırakış: {invoice.Transfer.DropoffAddress}";
                    if (!string.IsNullOrEmpty(invoice.Transfer.Note))
                        serviceDetails += $"\nNot: {invoice.Transfer.Note}";
                    discountPercentage = invoice.Transfer.DiscountPercentage;
                    originalPrice = invoice.Transfer.Price;
                }
                else if (invoice.CityTourId.HasValue && invoice.CityTour != null)
                {
                    invoiceType = "Şehir Turu Faturası";
                    serviceDescription = "Şehir Turu Hizmeti";
                    serviceDate = invoice.CityTour.TourDate;
                    serviceDetails = $"Dil: {invoice.CityTour.Language}\nSüre: {invoice.CityTour.DurationHours} saat";
                    discountPercentage = invoice.CityTour.DiscountPercentage;
                    originalPrice = invoice.CityTour.Price;
                }
                else if (invoice.YachtTourId.HasValue && invoice.YachtTour != null)
                {
                    invoiceType = "Yat Turu Faturası";
                    serviceDescription = "Yat Turu Hizmeti";
                    serviceDate = invoice.YachtTour.TourDate;
                    serviceDetails = $"Yat Adı: {invoice.YachtTour.YachtName}\nKişi Sayısı: {invoice.YachtTour.NumberOfPeople}";
                    if (!string.IsNullOrEmpty(invoice.YachtTour.SpecialRequest))
                        serviceDetails += $"\nÖzel İstek: {invoice.YachtTour.SpecialRequest}";
                    discountPercentage = invoice.YachtTour.DiscountPercentage;
                    originalPrice = invoice.YachtTour.Price;
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Row(row =>
                            {
                                row.ConstantItem(100).Text("GuestFlow")
                                    .FontSize(20)
                                    .FontFamily(Fonts.Calibri)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                row.RelativeItem().AlignRight().Text(invoiceType)
                                    .FontSize(16)
                                    .Bold();
                            });

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(20);

                                // Fatura Bilgileri
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("FATURA BİLGİLERİ").FontSize(12).Bold();
                                        col.Item().Text($"Fatura No: {invoice.InvoiceNumber}");
                                        col.Item().Text($"Tarih: {invoice.IssueDate:dd.MM.yyyy}");
                                        col.Item().Text($"Para Birimi: {invoice.Currency}");
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

                                // Hizmet Detayları
                                if (!string.IsNullOrEmpty(serviceDetails))
                                {
                                    column.Item().PaddingTop(10).Text("HİZMET DETAYLARI").FontSize(11).Bold();
                                    column.Item().Text(serviceDetails).FontSize(10);
                                    if (serviceDate.HasValue)
                                    {
                                        column.Item().PaddingTop(5).Text($"Tarih: {serviceDate.Value:dd.MM.yyyy HH:mm}").FontSize(10);
                                    }
                                }

                                // Fiyat Detayları
                                column.Item().PaddingTop(15).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Açıklama").Bold();
                                        header.Cell().Element(CellStyle).AlignRight().Text("Miktar").Bold();
                                        header.Cell().Element(CellStyle).AlignRight().Text("Tutar").Bold();
                                    });

                                    // Orijinal fiyat göster (indirim varsa)
                                    if (originalPrice.HasValue && discountPercentage.HasValue && discountPercentage.Value > 0)
                                    {
                                        table.Cell().Element(CellStyle).Text($"{serviceDescription} (İndirim Öncesi)");
                                        table.Cell().Element(CellStyle).AlignRight().Text("1");
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{originalPrice.Value:N2} {invoice.Currency}");
                                        
                                        table.Cell().Element(CellStyle).Text($"İndirim (%{discountPercentage.Value:N2})");
                                        table.Cell().Element(CellStyle).AlignRight().Text("-");
                                        var discountAmount = originalPrice.Value - invoice.TotalAmount;
                                        table.Cell().Element(CellStyle).AlignRight().Text($"-{discountAmount:N2} {invoice.Currency}").FontColor(Colors.Red.Medium);
                                    }
                                    else
                                    {
                                        table.Cell().Element(CellStyle).Text(serviceDescription);
                                        table.Cell().Element(CellStyle).AlignRight().Text("1");
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{invoice.TotalAmount:N2} {invoice.Currency}");
                                    }
                                });

                                // Toplam
                                column.Item().PaddingTop(10).AlignRight().Row(row =>
                                {
                                    row.ConstantItem(120).Text("TOPLAM:").FontSize(12).Bold();
                                    row.ConstantItem(120).AlignRight().Text($"{invoice.TotalAmount:N2} {invoice.Currency}").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                                });

                                // Notlar
                                if (!string.IsNullOrEmpty(invoice.Notes))
                                {
                                    column.Item().PaddingTop(10).Text("Notlar:").FontSize(10).Bold();
                                    column.Item().Text(invoice.Notes).FontSize(10);
                                }

                                // Personel Bilgisi
                                if (personnel != null)
                                {
                                    column.Item().PaddingTop(20).Text($"Hazırlayan: {personnel.FullName}").FontSize(9).Italic();
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("GuestFlow - Misafir Yönetim Sistemi")
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Medium);
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

