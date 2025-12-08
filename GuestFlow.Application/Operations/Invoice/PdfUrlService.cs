using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace GuestFlow.Application.Operations.Invoice
{
    public class PdfUrlService : IPdfUrlService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PdfUrlService> _logger;
        private readonly string _pdfStoragePath;
        private readonly string _baseUrl;

        public PdfUrlService(IConfiguration configuration, ILogger<PdfUrlService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _pdfStoragePath = _configuration["PdfSettings:StoragePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");
            _baseUrl = _configuration["PdfSettings:BaseUrl"] ?? "/invoices";
        }

        public string GetFileNameFromUrl(string pdfUrl)
        {
            if (string.IsNullOrWhiteSpace(pdfUrl))
                return string.Empty;

            try
            {
                // URL'den dosya adını çıkar
                // Örnek: "/invoices/invoice_1234_20240115103000.pdf" -> "invoice_1234_20240115103000.pdf"
                var uri = new Uri(pdfUrl, UriKind.RelativeOrAbsolute);
                var fileName = Path.GetFileName(uri.LocalPath);

                // Eğer URL "/invoices/" içeriyorsa, sadece dosya adını al
                if (pdfUrl.Contains("/invoices/"))
                {
                    var parts = pdfUrl.Split(new[] { "/invoices/" }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        fileName = Path.GetFileName(parts[1]);
                    }
                }

                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"PDF URL'den dosya adı çıkarılırken hata: {pdfUrl}");
                // Fallback: Basit string işleme
                return pdfUrl.Replace("/invoices/", "").Replace("\\invoices\\", "");
            }
        }

        public string GetFullFilePathFromUrl(string pdfUrl)
        {
            if (string.IsNullOrWhiteSpace(pdfUrl))
                return string.Empty;

            try
            {
                var fileName = GetFileNameFromUrl(pdfUrl);
                if (string.IsNullOrWhiteSpace(fileName))
                    return string.Empty;

                return Path.Combine(_pdfStoragePath, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PDF URL'den tam dosya yolu oluşturulurken hata: {pdfUrl}");
                return string.Empty;
            }
        }

        public string CreateUrlFromFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            try
            {
                // Base URL'den başlayarak URL oluştur
                var baseUrl = _baseUrl.TrimEnd('/');
                var cleanFileName = fileName.TrimStart('/').TrimStart('\\');
                return $"{baseUrl}/{cleanFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya adından PDF URL oluşturulurken hata: {fileName}");
                return string.Empty;
            }
        }

        public string GenerateFileName(int invoiceNumber)
        {
            return $"invoice_{invoiceNumber}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        }

        public bool IsValidPdfUrl(string pdfUrl)
        {
            if (string.IsNullOrWhiteSpace(pdfUrl))
                return false;

            try
            {
                // URL formatını kontrol et
                var fileName = GetFileNameFromUrl(pdfUrl);
                if (string.IsNullOrWhiteSpace(fileName))
                    return false;

                // Dosya adının .pdf ile bitip bitmediğini kontrol et
                return fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}

