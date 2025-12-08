using GuestFlow.Application.Types;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Import
{
    /// <summary>
    /// İçe aktarma servisi interface'i
    /// </summary>
    public interface IImportService
    {
        /// <summary>
        /// Excel dosyasından misafir listesini içe aktarır
        /// </summary>
        Task<ImportResult<ImportGuestDto>> ImportGuestsFromExcelAsync(IFormFile file);

        /// <summary>
        /// CSV dosyasından misafir listesini içe aktarır
        /// </summary>
        Task<ImportResult<ImportGuestDto>> ImportGuestsFromCsvAsync(IFormFile file);

        /// <summary>
        /// İçe aktarılan misafirleri veritabanına kaydeder
        /// </summary>
        Task<ServiceMessage<ImportSummaryDto>> SaveImportedGuestsAsync(List<ImportGuestDto> guests, bool skipDuplicates = true);
    }

    /// <summary>
    /// İçe aktarma sonucu
    /// </summary>
    public class ImportResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public List<T> Data { get; set; } = new List<T>();
        public List<ImportError> Errors { get; set; } = new List<ImportError>();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
    }

    /// <summary>
    /// İçe aktarma hatası
    /// </summary>
    public class ImportError
    {
        public int RowNumber { get; set; }
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? RowData { get; set; }
    }

    /// <summary>
    /// İçe aktarma özeti
    /// </summary>
    public class ImportSummaryDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int SkippedCount { get; set; }
        public List<ImportError> Errors { get; set; } = new List<ImportError>();
    }

    /// <summary>
    /// İçe aktarılacak misafir DTO
    /// </summary>
    public class ImportGuestDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Nationality { get; set; }
        public bool IsSpecialGuest { get; set; }
        public int RowNumber { get; set; }
        public bool IsValid { get; set; } = true;
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
}

