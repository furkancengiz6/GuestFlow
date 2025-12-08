namespace GuestFlow.Application.Configuration
{
    /// <summary>
    /// PDF ayarları
    /// </summary>
    public class PdfSettings
    {
        public string StoragePath { get; set; } = "wwwroot/invoices";
        public string BaseUrl { get; set; } = "/invoices";
        public string OutputPath { get; set; } = "wwwroot/pdfs";
    }
}

