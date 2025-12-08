using System;
using System.Collections.Generic;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Dosya metadata entity'si
    /// </summary>
    public class FileMetadataEntity : BaseEntity
    {
        /// <summary>
        /// Dosya adı (unique)
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Orijinal dosya adı
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// Dosya URL'i
        /// </summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>
        /// Dosya boyutu (bytes)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Content-Type
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Dosya kategorisi
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// İlişkili entity ID
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// İlişkili entity tipi
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// Dosya açıklaması
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Dosya etiketleri (virgülle ayrılmış)
        /// </summary>
        public string? Tags { get; set; }

        /// <summary>
        /// Azure Blob Container adı (eğer Azure kullanılıyorsa)
        /// </summary>
        public string? BlobContainerName { get; set; }

        /// <summary>
        /// Azure Blob adı (eğer Azure kullanılıyorsa)
        /// </summary>
        public string? BlobName { get; set; }

        /// <summary>
        /// Depolama tipi (Local, AzureBlob)
        /// </summary>
        public string StorageType { get; set; } = "Local";

        /// <summary>
        /// Yükleyen kullanıcı ID (Personnel)
        /// </summary>
        public int? UploadedByPersonnelId { get; set; }

        /// <summary>
        /// Yüklenme tarihi
        /// </summary>
        public DateTime UploadDate { get; set; }

        /// <summary>
        /// Son erişim tarihi
        /// </summary>
        public DateTime? LastAccessedDate { get; set; }

        /// <summary>
        /// Erişim sayısı
        /// </summary>
        public int AccessCount { get; set; }

        /// <summary>
        /// Özel metadata (JSON formatında)
        /// </summary>
        public string? CustomMetadata { get; set; }
    }
}

