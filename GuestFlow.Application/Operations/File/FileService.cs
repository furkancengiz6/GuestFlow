using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GuestFlow.Application.Operations.File.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.File
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;
        private readonly string _uploadPath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;
        private readonly bool _useAzureBlob;
        private readonly string? _azureConnectionString;
        private readonly string? _azureContainerName;
        private BlobServiceClient? _blobServiceClient;
        private BlobContainerClient? _blobContainerClient;
        private readonly Dictionary<string, FileMetadataDto> _fileMetadataCache = new Dictionary<string, FileMetadataDto>();

        public FileService(IConfiguration configuration, ILogger<FileService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _uploadPath = _configuration["FileSettings:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            _maxFileSize = long.Parse(_configuration["FileSettings:MaxFileSize"] ?? "10485760"); // 10MB default
            _allowedExtensions = _configuration["FileSettings:AllowedExtensions"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) 
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip" };

            // Azure Blob Storage yapılandırması
            _useAzureBlob = bool.Parse(_configuration["FileSettings:UseAzureBlob"] ?? "false");
            _azureConnectionString = _configuration["FileSettings:AzureConnectionString"];
            _azureContainerName = _configuration["FileSettings:AzureContainerName"] ?? "guestflow-files";

            if (_useAzureBlob && !string.IsNullOrEmpty(_azureConnectionString))
            {
                try
                {
                    _blobServiceClient = new BlobServiceClient(_azureConnectionString);
                    _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_azureContainerName);
                    _blobContainerClient.CreateIfNotExists();
                    _logger.LogInformation("Azure Blob Storage bağlantısı başarıyla kuruldu.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Azure Blob Storage bağlantısı kurulamadı. Yerel depolama kullanılacak.");
                    _useAzureBlob = false;
                }
            }

            // Upload klasörünü oluştur (yerel depolama için)
            if (!_useAzureBlob && !Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string? category = null, int? relatedEntityId = null, string? relatedEntityType = null, Dictionary<string, string>? metadata = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new FileUploadResult
                    {
                        IsSuccess = false,
                        Message = "Dosya seçilmedi veya dosya boş."
                    };
                }

                // Dosya boyutu kontrolü
                if (file.Length > _maxFileSize)
                {
                    return new FileUploadResult
                    {
                        IsSuccess = false,
                        Message = $"Dosya boyutu çok büyük. Maksimum boyut: {_maxFileSize / 1024 / 1024}MB"
                    };
                }

                // Dosya uzantısı kontrolü
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    return new FileUploadResult
                    {
                        IsSuccess = false,
                        Message = $"Bu dosya türü desteklenmiyor. İzin verilen uzantılar: {string.Join(", ", _allowedExtensions)}"
                    };
                }

                // Güvenli dosya adı oluştur
                var safeFileName = $"{Guid.NewGuid()}{fileExtension}";
                string fileUrl;

                if (_useAzureBlob && _blobContainerClient != null)
                {
                    // Azure Blob Storage'a yükle
                    var blobName = string.IsNullOrEmpty(category)
                        ? safeFileName
                        : $"{category}/{safeFileName}";

                    var blobClient = _blobContainerClient.GetBlobClient(blobName);

                    // Metadata ekle
                    var blobMetadata = new Dictionary<string, string>
                    {
                        { "OriginalFileName", file.FileName },
                        { "ContentType", file.ContentType },
                        { "UploadDate", DateTime.UtcNow.ToString("O") }
                    };

                    if (!string.IsNullOrEmpty(category))
                        blobMetadata["Category"] = category;
                    if (relatedEntityId.HasValue)
                        blobMetadata["RelatedEntityId"] = relatedEntityId.Value.ToString();
                    if (!string.IsNullOrEmpty(relatedEntityType))
                        blobMetadata["RelatedEntityType"] = relatedEntityType;
                    if (metadata != null)
                    {
                        foreach (var item in metadata)
                        {
                            blobMetadata[$"Custom_{item.Key}"] = item.Value;
                        }
                    }

                    using (var stream = file.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, new BlobUploadOptions
                        {
                            Metadata = blobMetadata,
                            HttpHeaders = new BlobHttpHeaders
                            {
                                ContentType = file.ContentType
                            }
                        });
                    }

                    fileUrl = blobClient.Uri.ToString();
                    _logger.LogInformation($"Dosya Azure Blob'a yüklendi: {blobName}, URL: {fileUrl}");
                }
                else
                {
                    // Yerel depolamaya yükle
                    var filePath = Path.Combine(_uploadPath, safeFileName);

                    // Kategori klasörü oluştur (varsa)
                    if (!string.IsNullOrEmpty(category))
                    {
                        var categoryPath = Path.Combine(_uploadPath, category);
                        if (!Directory.Exists(categoryPath))
                        {
                            Directory.CreateDirectory(categoryPath);
                        }
                        filePath = Path.Combine(categoryPath, safeFileName);
                    }

                    // Dosyayı kaydet
                    using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    fileUrl = string.IsNullOrEmpty(category)
                        ? $"/uploads/{safeFileName}"
                        : $"/uploads/{category}/{safeFileName}";

                    _logger.LogInformation($"Dosya yerel depolamaya yüklendi: {safeFileName}, Boyut: {file.Length} bytes, Kategori: {category ?? "Genel"}");
                }

                // Metadata kaydet
                if (metadata != null || !string.IsNullOrEmpty(category) || relatedEntityId.HasValue)
                {
                    var fileMetadata = new CreateFileMetadataDto
                    {
                        FileName = safeFileName,
                        OriginalFileName = file.FileName,
                        FileUrl = fileUrl,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        Category = category,
                        RelatedEntityId = relatedEntityId,
                        RelatedEntityType = relatedEntityType,
                        StorageType = _useAzureBlob ? "AzureBlob" : "Local",
                        BlobContainerName = _useAzureBlob ? _azureContainerName : null,
                        BlobName = _useAzureBlob ? (string.IsNullOrEmpty(category) ? safeFileName : $"{category}/{safeFileName}") : null,
                        CustomMetadata = metadata
                    };

                    await SaveFileMetadataAsync(fileMetadata);
                }

                return new FileUploadResult
                {
                    IsSuccess = true,
                    Message = "Dosya başarıyla yüklendi.",
                    FileName = safeFileName,
                    FileUrl = fileUrl,
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya yüklenirken hata: {ex.Message}");
                return new FileUploadResult
                {
                    IsSuccess = false,
                    Message = $"Dosya yüklenirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<FileDownloadResult> DownloadFileAsync(string fileName)
        {
            try
            {
                // Güvenlik: Dosya adında path traversal saldırılarını önle
                if (fileName.Contains("..") || (fileName.Contains("/") && !_useAzureBlob) || fileName.Contains("\\"))
                {
                    return new FileDownloadResult
                    {
                        IsSuccess = false,
                        Message = "Geçersiz dosya adı."
                    };
                }

                byte[] fileContent;
                string contentType;

                if (_useAzureBlob && _blobContainerClient != null)
                {
                    // Azure Blob'dan indir
                    var metadata = await GetFileMetadataAsync(fileName);
                    var blobName = metadata?.BlobName ?? fileName;

                    var blobClient = _blobContainerClient.GetBlobClient(blobName);
                    if (!await blobClient.ExistsAsync())
                    {
                        return new FileDownloadResult
                        {
                            IsSuccess = false,
                            Message = "Dosya bulunamadı."
                        };
                    }

                    var blobDownloadInfo = await blobClient.DownloadAsync();
                    using (var memoryStream = new MemoryStream())
                    {
                        await blobDownloadInfo.Value.Content.CopyToAsync(memoryStream);
                        fileContent = memoryStream.ToArray();
                    }

                    contentType = blobDownloadInfo.Value.ContentType ?? GetContentType(fileName);

                    // Metadata güncelle (erişim sayısı)
                    if (metadata != null)
                    {
                        metadata.AccessCount++;
                        metadata.LastAccessedDate = DateTime.UtcNow;
                        _fileMetadataCache[fileName] = metadata;
                    }
                }
                else
                {
                    // Yerel depolamadan indir
                    var filePath = Path.Combine(_uploadPath, fileName);

                    // Kategori klasörlerinde de ara
                    if (!System.IO.File.Exists(filePath))
                    {
                        var categoryDirs = Directory.GetDirectories(_uploadPath);
                        foreach (var categoryDir in categoryDirs)
                        {
                            var categoryFilePath = Path.Combine(categoryDir, fileName);
                            if (System.IO.File.Exists(categoryFilePath))
                            {
                                filePath = categoryFilePath;
                                break;
                            }
                        }
                    }

                    if (!System.IO.File.Exists(filePath))
                    {
                        return new FileDownloadResult
                        {
                            IsSuccess = false,
                            Message = "Dosya bulunamadı."
                        };
                    }

                    fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
                    contentType = GetContentType(fileName);

                    // Metadata güncelle (erişim sayısı)
                    var localMetadata = await GetFileMetadataAsync(fileName);
                    if (localMetadata != null)
                    {
                        localMetadata.AccessCount++;
                        localMetadata.LastAccessedDate = DateTime.UtcNow;
                        _fileMetadataCache[fileName] = localMetadata;
                    }
                }

                return new FileDownloadResult
                {
                    IsSuccess = true,
                    Message = "Dosya başarıyla indirildi.",
                    FileContent = fileContent,
                    FileName = fileName,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya indirilirken hata: {ex.Message}");
                return new FileDownloadResult
                {
                    IsSuccess = false,
                    Message = $"Dosya indirilirken hata oluştu: {ex.Message}"
                };
            }
        }

        public Task<List<FileInfoDto>> GetFilesAsync(string? category = null, int? relatedEntityId = null, string? relatedEntityType = null)
        {
            try
            {
                var files = new List<FileInfoDto>();
                var searchPath = string.IsNullOrEmpty(category)
                    ? _uploadPath
                    : Path.Combine(_uploadPath, category);

                if (!Directory.Exists(searchPath))
                {
                    return Task.FromResult(files);
                }

                var fileEntries = Directory.GetFiles(searchPath);
                foreach (var filePath in fileEntries)
                {
                    var fileInfo = new System.IO.FileInfo(filePath);
                    var fileName = fileInfo.Name;
                    var fileUrl = string.IsNullOrEmpty(category)
                        ? $"/uploads/{fileName}"
                        : $"/uploads/{category}/{fileName}";

                    files.Add(new FileInfoDto
                    {
                        FileName = fileName,
                        FileUrl = fileUrl,
                        FileSize = fileInfo.Length,
                        ContentType = GetContentType(fileName),
                        Category = category,
                        RelatedEntityId = relatedEntityId,
                        RelatedEntityType = relatedEntityType,
                        UploadDate = fileInfo.CreationTime
                    });
                }

                return Task.FromResult(files.OrderByDescending(f => f.UploadDate).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya listesi alınırken hata: {ex.Message}");
                return Task.FromResult(new List<FileInfoDto>());
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                // Güvenlik: Dosya adında path traversal saldırılarını önle
                if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    _logger.LogWarning($"Geçersiz dosya adı: {fileName}");
                    return false;
                }

                var filePath = Path.Combine(_uploadPath, fileName);

                // Kategori klasörlerinde de ara
                if (!System.IO.File.Exists(filePath))
                {
                    var categoryDirs = Directory.GetDirectories(_uploadPath);
                    foreach (var categoryDir in categoryDirs)
                    {
                        var categoryFilePath = Path.Combine(categoryDir, fileName);
                        if (System.IO.File.Exists(categoryFilePath))
                        {
                            filePath = categoryFilePath;
                            break;
                        }
                    }
                }

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"Dosya bulunamadı: {fileName}");
                    return false;
                }

                System.IO.File.Delete(filePath);
                _logger.LogInformation($"Dosya silindi: {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya silinirken hata: {ex.Message}");
                return false;
            }
        }

        public Task<FileInfoDto?> GetFileInfoAsync(string fileName)
        {
            try
            {
                // Güvenlik: Dosya adında path traversal saldırılarını önle
                if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return Task.FromResult<FileInfoDto?>(null);
                }

                var filePath = Path.Combine(_uploadPath, fileName);

                // Kategori klasörlerinde de ara
                if (!System.IO.File.Exists(filePath))
                {
                    var categoryDirs = Directory.GetDirectories(_uploadPath);
                    foreach (var categoryDir in categoryDirs)
                    {
                        var categoryFilePath = Path.Combine(categoryDir, fileName);
                        if (System.IO.File.Exists(categoryFilePath))
                        {
                            filePath = categoryFilePath;
                            var category = Path.GetFileName(categoryDir);
                            var fileInfo = new System.IO.FileInfo(filePath);
                            return Task.FromResult<FileInfoDto?>(new FileInfoDto
                            {
                                FileName = fileInfo.Name,
                                FileUrl = $"/uploads/{category}/{fileInfo.Name}",
                                FileSize = fileInfo.Length,
                                ContentType = GetContentType(fileInfo.Name),
                                Category = category,
                                UploadDate = fileInfo.CreationTime
                            });
                        }
                    }
                }

                if (!System.IO.File.Exists(filePath))
                {
                    return Task.FromResult<FileInfoDto?>(null);
                }

                var info = new System.IO.FileInfo(filePath);
                return Task.FromResult<FileInfoDto?>(new FileInfoDto
                {
                    FileName = info.Name,
                    FileUrl = $"/uploads/{info.Name}",
                    FileSize = info.Length,
                    ContentType = GetContentType(info.Name),
                    UploadDate = info.CreationTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya bilgisi alınırken hata: {ex.Message}");
                return Task.FromResult<FileInfoDto?>(null);
            }
        }

        public Task<List<FileCategoryDto>> GetFileCategoriesAsync()
        {
            try
            {
                var categories = new List<FileCategoryDto>();
                var categoryDescriptions = new Dictionary<string, string>
                {
                    { "invoices", "Fatura PDF'leri" },
                    { "guests", "Misafir belgeleri" },
                    { "tours", "Tur görselleri" },
                    { "transfers", "Transfer belgeleri" },
                    { "general", "Genel dosyalar" }
                };

                if (!Directory.Exists(_uploadPath))
                {
                    return Task.FromResult(categories);
                }

                // Ana klasördeki dosyalar
                var rootFiles = Directory.GetFiles(_uploadPath);
                if (rootFiles.Length > 0)
                {
                    var totalSize = rootFiles.Sum(f => new System.IO.FileInfo(f).Length);
                    categories.Add(new FileCategoryDto
                    {
                        Category = "general",
                        FileCount = rootFiles.Length,
                        TotalSize = totalSize,
                        Description = categoryDescriptions.GetValueOrDefault("general", "Genel dosyalar")
                    });
                }

                // Kategori klasörleri
                var categoryDirs = Directory.GetDirectories(_uploadPath);
                foreach (var categoryDir in categoryDirs)
                {
                    var categoryName = Path.GetFileName(categoryDir);
                    var files = Directory.GetFiles(categoryDir);
                    var totalSize = files.Sum(f => new System.IO.FileInfo(f).Length);

                    categories.Add(new FileCategoryDto
                    {
                        Category = categoryName,
                        FileCount = files.Length,
                        TotalSize = totalSize,
                        Description = categoryDescriptions.GetValueOrDefault(categoryName, $"{categoryName} kategorisi")
                    });
                }

                return Task.FromResult(categories.OrderByDescending(c => c.FileCount).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya kategorileri alınırken hata: {ex.Message}");
                return Task.FromResult(new List<FileCategoryDto>());
            }
        }

        public Task<FileStatisticsDto> GetFileStatisticsAsync()
        {
            try
            {
                var statistics = new FileStatisticsDto();
                var filesByCategory = new Dictionary<string, int>();
                var sizeByCategory = new Dictionary<string, long>();
                var filesByType = new Dictionary<string, int>();

                if (!Directory.Exists(_uploadPath))
                {
                    return Task.FromResult(statistics);
                }

                // Ana klasördeki dosyalar
                var rootFiles = Directory.GetFiles(_uploadPath);
                foreach (var filePath in rootFiles)
                {
                    var fileInfo = new System.IO.FileInfo(filePath);
                    var extension = Path.GetExtension(filePath).ToLowerInvariant();
                    var fileType = GetFileType(extension);

                    statistics.TotalFiles++;
                    statistics.TotalSize += fileInfo.Length;

                    filesByCategory["general"] = filesByCategory.GetValueOrDefault("general", 0) + 1;
                    sizeByCategory["general"] = sizeByCategory.GetValueOrDefault("general", 0) + fileInfo.Length;
                    filesByType[fileType] = filesByType.GetValueOrDefault(fileType, 0) + 1;
                }

                // Kategori klasörleri
                var categoryDirs = Directory.GetDirectories(_uploadPath);
                foreach (var categoryDir in categoryDirs)
                {
                    var categoryName = Path.GetFileName(categoryDir);
                    var files = Directory.GetFiles(categoryDir);

                    foreach (var filePath in files)
                    {
                        var fileInfo = new System.IO.FileInfo(filePath);
                        var extension = Path.GetExtension(filePath).ToLowerInvariant();
                        var fileType = GetFileType(extension);

                        statistics.TotalFiles++;
                        statistics.TotalSize += fileInfo.Length;

                        filesByCategory[categoryName] = filesByCategory.GetValueOrDefault(categoryName, 0) + 1;
                        sizeByCategory[categoryName] = sizeByCategory.GetValueOrDefault(categoryName, 0) + fileInfo.Length;
                        filesByType[fileType] = filesByType.GetValueOrDefault(fileType, 0) + 1;
                    }
                }

                statistics.FilesByCategory = filesByCategory;
                statistics.SizeByCategory = sizeByCategory;
                statistics.FilesByType = filesByType;

                return Task.FromResult(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya istatistikleri alınırken hata: {ex.Message}");
                return Task.FromResult(new FileStatisticsDto());
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        private string GetFileType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" => "Image",
                ".pdf" => "PDF",
                ".doc" or ".docx" => "Document",
                ".xls" or ".xlsx" => "Spreadsheet",
                ".txt" => "Text",
                ".zip" => "Archive",
                _ => "Other"
            };
        }

        public Task<FileMetadataDto> SaveFileMetadataAsync(CreateFileMetadataDto metadata)
        {
            try
            {
                var fileMetadata = new FileMetadataDto
                {
                    FileName = metadata.FileName,
                    OriginalFileName = metadata.OriginalFileName,
                    FileUrl = metadata.FileUrl,
                    FileSize = metadata.FileSize,
                    ContentType = metadata.ContentType,
                    Category = metadata.Category,
                    RelatedEntityId = metadata.RelatedEntityId,
                    RelatedEntityType = metadata.RelatedEntityType,
                    Description = metadata.Description,
                    Tags = metadata.Tags ?? new List<string>(),
                    StorageType = metadata.StorageType,
                    BlobContainerName = metadata.BlobContainerName,
                    BlobName = metadata.BlobName,
                    UploadDate = DateTime.UtcNow,
                    CustomMetadata = metadata.CustomMetadata
                };

                _fileMetadataCache[metadata.FileName] = fileMetadata;
                _logger.LogInformation($"Dosya metadata kaydedildi: {metadata.FileName}");

                return Task.FromResult(fileMetadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya metadata kaydedilirken hata: {ex.Message}");
                throw;
            }
        }

        public Task<FileMetadataDto?> GetFileMetadataAsync(string fileName)
        {
            try
            {
                if (_fileMetadataCache.TryGetValue(fileName, out var cachedMetadata))
                {
                    return Task.FromResult<FileMetadataDto?>(cachedMetadata);
                }

                // Cache'de yoksa, dosya bilgisinden oluştur
                var fileInfo = GetFileInfoAsync(fileName).Result;
                if (fileInfo == null)
                {
                    return Task.FromResult<FileMetadataDto?>(null);
                }

                var metadata = new FileMetadataDto
                {
                    FileName = fileInfo.FileName,
                    OriginalFileName = fileInfo.FileName,
                    FileUrl = fileInfo.FileUrl,
                    FileSize = fileInfo.FileSize,
                    ContentType = fileInfo.ContentType,
                    Category = fileInfo.Category,
                    RelatedEntityId = fileInfo.RelatedEntityId,
                    RelatedEntityType = fileInfo.RelatedEntityType,
                    UploadDate = fileInfo.UploadDate,
                    StorageType = _useAzureBlob ? "AzureBlob" : "Local",
                    BlobContainerName = _useAzureBlob ? _azureContainerName : null,
                    BlobName = null
                };

                _fileMetadataCache[fileName] = metadata;
                return Task.FromResult<FileMetadataDto?>(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya metadata getirilirken hata: {ex.Message}");
                return Task.FromResult<FileMetadataDto?>(null);
            }
        }

        public Task<FileMetadataDto?> UpdateFileMetadataAsync(string fileName, UpdateFileMetadataDto metadata)
        {
            try
            {
                var existingMetadata = GetFileMetadataAsync(fileName).Result;
                if (existingMetadata == null)
                {
                    return Task.FromResult<FileMetadataDto?>(null);
                }

                if (!string.IsNullOrEmpty(metadata.Description))
                    existingMetadata.Description = metadata.Description;

                if (metadata.Tags != null)
                    existingMetadata.Tags = metadata.Tags;

                if (metadata.CustomMetadata != null)
                    existingMetadata.CustomMetadata = metadata.CustomMetadata;

                _fileMetadataCache[fileName] = existingMetadata;
                _logger.LogInformation($"Dosya metadata güncellendi: {fileName}");

                return Task.FromResult<FileMetadataDto?>(existingMetadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya metadata güncellenirken hata: {ex.Message}");
                return Task.FromResult<FileMetadataDto?>(null);
            }
        }

        public async Task<FileDownloadResult> GetFilePreviewAsync(string fileName, int? width = null, int? height = null)
        {
            try
            {
                var downloadResult = await DownloadFileAsync(fileName);
                if (!downloadResult.IsSuccess || downloadResult.FileContent == null)
                {
                    return downloadResult;
                }

                // Sadece görseller için önizleme
                var contentType = downloadResult.ContentType?.ToLower() ?? "";
                if (!contentType.StartsWith("image/"))
                {
                    return new FileDownloadResult
                    {
                        IsSuccess = false,
                        Message = "Önizleme sadece görseller için destekleniyor."
                    };
                }

                // ImageSharp ile görseli yeniden boyutlandır
                using (var image = Image.Load(downloadResult.FileContent))
                {
                    if (width.HasValue || height.HasValue)
                    {
                        var resizeOptions = new ResizeOptions
                        {
                            Size = new Size(width ?? 0, height ?? 0),
                            Mode = ResizeMode.Max
                        };

                        if (!width.HasValue)
                            resizeOptions.Size = new Size(image.Width, height.Value);
                        else if (!height.HasValue)
                            resizeOptions.Size = new Size(width.Value, image.Height);

                        image.Mutate(x => x.Resize(resizeOptions));
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await image.SaveAsJpegAsync(memoryStream);
                        return new FileDownloadResult
                        {
                            IsSuccess = true,
                            Message = "Önizleme başarıyla oluşturuldu.",
                            FileContent = memoryStream.ToArray(),
                            FileName = $"preview_{fileName}",
                            ContentType = "image/jpeg"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya önizlemesi oluşturulurken hata: {ex.Message}");
                return new FileDownloadResult
                {
                    IsSuccess = false,
                    Message = $"Önizleme oluşturulurken hata oluştu: {ex.Message}"
                };
            }
        }
    }
}

