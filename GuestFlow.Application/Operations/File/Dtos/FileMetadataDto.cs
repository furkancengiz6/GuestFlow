using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.File.Dtos
{
    /// <summary>
    /// Dosya metadata DTO
    /// </summary>
    public class FileMetadataDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? Description { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string StorageType { get; set; } = "Local";
        public string? BlobContainerName { get; set; }
        public string? BlobName { get; set; }
        public int? UploadedByPersonnelId { get; set; }
        public string? UploadedByPersonnelName { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime? LastAccessedDate { get; set; }
        public int AccessCount { get; set; }
        public Dictionary<string, string>? CustomMetadata { get; set; }
    }

    /// <summary>
    /// Dosya metadata oluşturma/güncelleme DTO
    /// </summary>
    public class CreateFileMetadataDto
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? Description { get; set; }
        public List<string>? Tags { get; set; }
        public string StorageType { get; set; } = "Local";
        public string? BlobContainerName { get; set; }
        public string? BlobName { get; set; }
        public Dictionary<string, string>? CustomMetadata { get; set; }
    }

    /// <summary>
    /// Dosya metadata güncelleme DTO
    /// </summary>
    public class UpdateFileMetadataDto
    {
        public string? Description { get; set; }
        public List<string>? Tags { get; set; }
        public Dictionary<string, string>? CustomMetadata { get; set; }
    }
}

