namespace GuestFlow.Application.Operations.Invoice
{
    /// <summary>
    /// PDF URL yönetimi servisi
    /// </summary>
    public interface IPdfUrlService
    {
        /// <summary>
        /// PDF URL'den dosya adını çıkarır
        /// </summary>
        string GetFileNameFromUrl(string pdfUrl);

        /// <summary>
        /// PDF URL'den tam dosya yolunu oluşturur
        /// </summary>
        string GetFullFilePathFromUrl(string pdfUrl);

        /// <summary>
        /// Dosya adından PDF URL oluşturur
        /// </summary>
        string CreateUrlFromFileName(string fileName);

        /// <summary>
        /// Fatura numarasından PDF dosya adı oluşturur
        /// </summary>
        string GenerateFileName(int invoiceNumber);

        /// <summary>
        /// PDF URL'nin geçerli olup olmadığını kontrol eder
        /// </summary>
        bool IsValidPdfUrl(string pdfUrl);
    }
}

