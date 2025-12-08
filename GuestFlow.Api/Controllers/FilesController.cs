using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.File;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [Tags("Dosyalar")]
    public class FilesController : BaseController
    {
        private readonly IFileService _fileService;
        private readonly IFileShareService _fileShareService;

        public FilesController(IFileService fileService, IFileShareService fileShareService)
        {
            _fileService = fileService;
            _fileShareService = fileShareService;
        }

        /// <summary>
        /// Dosya yükler
        /// </summary>
        /// <remarks>
        /// Bu endpoint multipart/form-data formatında dosya yükleme yapar.
        /// Swagger UI'da test etmek için "Try it out" butonuna tıklayın ve dosya seçin.
        /// </remarks>
        /// <param name="file">Yüklenecek dosya</param>
        /// <param name="category">Dosya kategorisi (opsiyonel)</param>
        /// <param name="relatedEntityId">İlişkili entity ID (opsiyonel)</param>
        /// <param name="relatedEntityType">İlişkili entity tipi (opsiyonel)</param>
        /// <returns>Yüklenen dosya bilgileri</returns>
        /// <response code="200">Dosya başarıyla yüklendi</response>
        /// <response code="400">Dosya seçilmedi veya geçersiz</response>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UploadFile(
            IFormFile file,
            [FromForm] string? category = null,
            [FromForm] int? relatedEntityId = null,
            [FromForm] string? relatedEntityType = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Error("Dosya seçilmedi.", 400);
                }

                var result = await _fileService.UploadFileAsync(file, category, relatedEntityId, relatedEntityType);

                if (result.IsSuccess)
                {
                    return Success(new
                    {
                        FileName = result.FileName,
                        FileUrl = result.FileUrl,
                        FileSize = result.FileSize,
                        ContentType = result.ContentType
                    }, result.Message);
                }

                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Dosya yüklenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Birden fazla dosya yükler
        /// </summary>
        /// <remarks>
        /// Bu endpoint birden fazla dosyayı aynı anda yükler.
        /// Swagger UI'da test etmek için "Try it out" butonuna tıklayın ve dosyaları seçin.
        /// </remarks>
        /// <param name="files">Yüklenecek dosya listesi</param>
        /// <param name="category">Dosya kategorisi (opsiyonel)</param>
        /// <param name="relatedEntityId">İlişkili entity ID (opsiyonel)</param>
        /// <param name="relatedEntityType">İlişkili entity tipi (opsiyonel)</param>
        /// <returns>Yüklenen dosya bilgileri ve hata listesi</returns>
        /// <response code="200">Dosyalar yüklendi (başarılı ve başarısız olanlar ayrı listelenir)</response>
        /// <response code="400">Dosya seçilmedi veya geçersiz</response>
        [HttpPost("upload/bulk")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UploadFiles(
            List<IFormFile> files,
            [FromForm] string? category = null,
            [FromForm] int? relatedEntityId = null,
            [FromForm] string? relatedEntityType = null)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return Error("Dosya seçilmedi.", 400);
                }

                var results = new List<object>();
                var errors = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var result = await _fileService.UploadFileAsync(file, category, relatedEntityId, relatedEntityType);
                        if (result.IsSuccess)
                        {
                            results.Add(new
                            {
                                FileName = result.FileName,
                                FileUrl = result.FileUrl,
                                FileSize = result.FileSize,
                                ContentType = result.ContentType
                            });
                        }
                        else
                        {
                            errors.Add($"{file.FileName}: {result.Message}");
                        }
                    }
                }

                if (errors.Count > 0)
                {
                    return Success(new
                    {
                        Successful = results,
                        Failed = errors
                    }, $"{results.Count} dosya başarıyla yüklendi, {errors.Count} dosya başarısız oldu.");
                }

                return Success(results, $"{results.Count} dosya başarıyla yüklendi.");
            }
            catch (Exception ex)
            {
                return Error("Dosyalar yüklenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Dosyayı indirir
        /// </summary>
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var result = await _fileService.DownloadFileAsync(fileName);

            if (result.IsSuccess && result.FileContent != null)
            {
                return File(result.FileContent, result.ContentType ?? "application/octet-stream", result.FileName ?? fileName);
            }

            return NotFound(new { Message = result.Message });
        }

        /// <summary>
        /// Dosya listesini getirir (sayfalama, filtreleme, sıralama ile)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFiles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] int? relatedEntityId = null,
            [FromQuery] string? relatedEntityType = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc")
        {
            try
            {
                var files = await _fileService.GetFilesAsync(category, relatedEntityId, relatedEntityType);
                
                // Arama terimi filtresi
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var search = searchTerm.ToLower();
                    files = files.Where(f => 
                        f.FileName.ToLower().Contains(search) ||
                        (f.ContentType != null && f.ContentType.ToLower().Contains(search))
                    ).ToList();
                }

                // Sıralama
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    var isAscending = sortOrder?.ToLower() == "asc";
                    files = sortBy.ToLower() switch
                    {
                        "filename" or "name" => isAscending 
                            ? files.OrderBy(f => f.FileName).ToList() 
                            : files.OrderByDescending(f => f.FileName).ToList(),
                        "size" => isAscending 
                            ? files.OrderBy(f => f.FileSize).ToList() 
                            : files.OrderByDescending(f => f.FileSize).ToList(),
                        "date" or "uploaddate" => isAscending 
                            ? files.OrderBy(f => f.UploadDate).ToList() 
                            : files.OrderByDescending(f => f.UploadDate).ToList(),
                        "category" => isAscending 
                            ? files.OrderBy(f => f.Category ?? "").ToList() 
                            : files.OrderByDescending(f => f.Category ?? "").ToList(),
                        _ => files.OrderByDescending(f => f.UploadDate).ToList()
                    };
                }
                else
                {
                    files = files.OrderByDescending(f => f.UploadDate).ToList();
                }

                // Sayfalama
                var totalCount = files.Count;
                var pagedFiles = files.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var pagedResult = new GuestFlow.Application.Models.PagedResult<FileInfoDto>(pagedFiles, totalCount, pageNumber, pageSize);

                return PagedResult(pagedResult, "Dosyalar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Dosya listesi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Dosya bilgisini getirir
        /// </summary>
        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetFileInfo(string fileName)
        {
            try
            {
                var fileInfo = await _fileService.GetFileInfoAsync(fileName);

                if (fileInfo == null)
                {
                    return Error("Dosya bulunamadı.", 404);
                }

                return Success(fileInfo, "Dosya bilgisi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Dosya bilgisi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Dosya kategorilerini getirir
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetFileCategories()
        {
            try
            {
                var categories = await _fileService.GetFileCategoriesAsync();
                return Success(categories, "Dosya kategorileri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Dosya kategorileri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Dosya istatistiklerini getirir
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFileStatistics()
        {
            try
            {
                var statistics = await _fileService.GetFileStatisticsAsync();
                return Success(statistics, "Dosya istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Dosya istatistikleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Fatura PDF'lerini getirir
        /// </summary>
        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoiceFiles(
            [FromQuery] int? invoiceId = null)
        {
            try
            {
                var files = await _fileService.GetFilesAsync("invoices", invoiceId, "Invoice");
                return Success(files, "Fatura dosyaları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Fatura dosyaları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafir belgelerini getirir
        /// </summary>
        [HttpGet("guests/{guestId}")]
        public async Task<IActionResult> GetGuestFiles(int guestId)
        {
            try
            {
                var files = await _fileService.GetFilesAsync("guests", guestId, "Guest");
                return Success(files, "Misafir dosyaları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir dosyaları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tur görsellerini getirir
        /// </summary>
        [HttpGet("tours/{tourId}")]
        public async Task<IActionResult> GetTourFiles(int tourId, [FromQuery] string? tourType = null)
        {
            try
            {
                var entityType = tourType?.ToLower() == "yacht" ? "YachtTour" : "CityTour";
                var files = await _fileService.GetFilesAsync("tours", tourId, entityType);
                return Success(files, "Tur dosyaları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Tur dosyaları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Dosyayı siler
        /// </summary>
        [HttpDelete("{fileName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                var result = await _fileService.DeleteFileAsync(fileName);

                if (result)
                {
                    return Success("Dosya başarıyla silindi.");
                }

                return Error("Dosya bulunamadı veya silinemedi.", 404);
            }
            catch (Exception ex)
            {
                return Error("Dosya silinirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Dosya metadata'sını getirir
        /// </summary>
        [HttpGet("{fileName}/metadata")]
        public async Task<IActionResult> GetFileMetadata(string fileName)
        {
            try
            {
                var metadata = await _fileService.GetFileMetadataAsync(fileName);
                if (metadata == null)
                {
                    return Error("Dosya metadata'sı bulunamadı.", 404);
                }

                return Success(metadata, "Dosya metadata'sı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Dosya metadata'sı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Dosya metadata'sını günceller
        /// </summary>
        [HttpPut("{fileName}/metadata")]
        public async Task<IActionResult> UpdateFileMetadata(string fileName, [FromBody] GuestFlow.Application.Operations.File.Dtos.UpdateFileMetadataDto metadata)
        {
            try
            {
                var result = await _fileService.UpdateFileMetadataAsync(fileName, metadata);
                if (result == null)
                {
                    return Error("Dosya metadata'sı bulunamadı.", 404);
                }

                return Success(result, "Dosya metadata'sı başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return Error("Dosya metadata'sı güncellenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Dosya önizlemesini getirir (görseller için)
        /// </summary>
        [HttpGet("{fileName}/preview")]
        public async Task<IActionResult> GetFilePreview(
            string fileName,
            [FromQuery] int? width = null,
            [FromQuery] int? height = null)
        {
            try
            {
                var result = await _fileService.GetFilePreviewAsync(fileName, width, height);
                if (!result.IsSuccess || result.FileContent == null)
                {
                    return Error(result.Message, 400);
                }

                return File(result.FileContent, result.ContentType ?? "image/jpeg", result.FileName ?? fileName);
            }
            catch (Exception ex)
            {
                return Error("Dosya önizlemesi oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Dosya için paylaşım linki oluşturur
        /// </summary>
        [HttpPost("{fileName}/share")]
        public async Task<IActionResult> CreateShareLink(
            string fileName,
            [FromBody] CreateShareLinkRequest? request = null)
        {
            try
            {
                var expirationHours = request?.ExpirationHours ?? 24;
                var password = request?.Password;

                var shareLink = await _fileShareService.CreateShareLinkAsync(fileName, expirationHours, password);
                return Success(shareLink, "Paylaşım linki başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return Error("Paylaşım linki oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Paylaşım linki ile dosyayı indirir
        /// </summary>
        [HttpGet("share/{shareToken}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFileByShareLink(
            string shareToken,
            [FromQuery] string? password = null)
        {
            try
            {
                var shareLink = await _fileShareService.ValidateShareLinkAsync(shareToken);
                if (shareLink == null)
                {
                    return Error("Geçersiz veya süresi dolmuş paylaşım linki.", 404);
                }

                // Şifre kontrolü
                if (shareLink.HasPassword)
                {
                    if (string.IsNullOrEmpty(password) || password != shareLink.Password)
                    {
                        return Error("Paylaşım linki için şifre gereklidir.", 401);
                    }
                }

                var downloadResult = await _fileService.DownloadFileAsync(shareLink.FileName);
                if (!downloadResult.IsSuccess || downloadResult.FileContent == null)
                {
                    return Error(downloadResult.Message, 404);
                }

                return File(downloadResult.FileContent, downloadResult.ContentType ?? "application/octet-stream", downloadResult.FileName ?? shareLink.FileName);
            }
            catch (Exception ex)
            {
                return Error("Dosya indirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Paylaşım linkini iptal eder
        /// </summary>
        [HttpDelete("share/{shareToken}")]
        public async Task<IActionResult> RevokeShareLink(string shareToken)
        {
            try
            {
                var result = await _fileShareService.RevokeShareLinkAsync(shareToken);
                if (result)
                {
                    return Success("Paylaşım linki başarıyla iptal edildi.");
                }

                return Error("Paylaşım linki bulunamadı.", 404);
            }
            catch (Exception ex)
            {
                return Error("Paylaşım linki iptal edilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Aktif paylaşım linklerini getirir
        /// </summary>
        [HttpGet("share")]
        public async Task<IActionResult> GetActiveShareLinks([FromQuery] string? fileName = null)
        {
            try
            {
                var shareLinks = await _fileShareService.GetActiveShareLinksAsync(fileName);
                return Success(shareLinks, "Aktif paylaşım linkleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Paylaşım linkleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Paylaşım linki oluşturma isteği
    /// </summary>
    public class CreateShareLinkRequest
    {
        public int ExpirationHours { get; set; } = 24;
        public string? Password { get; set; }
    }
}

