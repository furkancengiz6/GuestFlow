namespace GuestFlow.Application.Configuration
{
    /// <summary>
    /// Dosya yükleme ayarları
    /// </summary>
    public class FileSettings
    {
        public string UploadPath { get; set; } = "wwwroot/uploads";
        public long MaxFileSize { get; set; } = 10485760; // 10MB
        public string AllowedExtensions { get; set; } = ".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx,.txt,.zip";
        public bool UseAzureBlob { get; set; } = false;
        public string AzureConnectionString { get; set; } = string.Empty;
        public string AzureContainerName { get; set; } = "guestflow-files";
        public string BaseUrl { get; set; } = string.Empty;
    }
}

