using GuestFlow.Application.Operations.File.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.File
{
    public interface IFileService
    {
        /// <summary>
        /// Dosya yükler ve bilgilerini döndürür
        /// </summary>
        Task<FileUploadResult> UploadFileAsync(Microsoft.AspNetCore.Http.IFormFile file, string? category = null, int? relatedEntityId = null, string? relatedEntityType = null, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Dosyayı indirir
        /// </summary>
        Task<FileDownloadResult> DownloadFileAsync(string fileName);

        /// <summary>
        /// Dosya listesini getirir
        /// </summary>
        Task<List<FileInfoDto>> GetFilesAsync(string? category = null, int? relatedEntityId = null, string? relatedEntityType = null);

        /// <summary>
        /// Dosyayı siler
        /// </summary>
        Task<bool> DeleteFileAsync(string fileName);

        /// <summary>
        /// Dosya bilgisini getirir
        /// </summary>
        Task<FileInfoDto?> GetFileInfoAsync(string fileName);
        
        /// <summary>
        /// Dosya kategorilerini getirir
        /// </summary>
        Task<List<FileCategoryDto>> GetFileCategoriesAsync();
        
        /// <summary>
        /// Dosya istatistiklerini getirir
        /// </summary>
        Task<FileStatisticsDto> GetFileStatisticsAsync();

        /// <summary>
        /// Dosya metadata'sını oluşturur veya günceller
        /// </summary>
        Task<FileMetadataDto> SaveFileMetadataAsync(CreateFileMetadataDto metadata);

        /// <summary>
        /// Dosya metadata'sını getirir
        /// </summary>
        Task<FileMetadataDto?> GetFileMetadataAsync(string fileName);

        /// <summary>
        /// Dosya metadata'sını günceller
        /// </summary>
        Task<FileMetadataDto?> UpdateFileMetadataAsync(string fileName, UpdateFileMetadataDto metadata);

        /// <summary>
        /// Dosya önizlemesini getirir (görseller için)
        /// </summary>
        Task<FileDownloadResult> GetFilePreviewAsync(string fileName, int? width = null, int? height = null);
    }

    public class FileUploadResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
    }

    public class FileDownloadResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public byte[]? FileContent { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }

    public class FileInfoDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public DateTime UploadDate { get; set; }
    }

    public class FileCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public long TotalSize { get; set; }
        public string? Description { get; set; }
    }

    public class FileStatisticsDto
    {
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public Dictionary<string, int> FilesByCategory { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, long> SizeByCategory { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, int> FilesByType { get; set; } = new Dictionary<string, int>();
    }
}

