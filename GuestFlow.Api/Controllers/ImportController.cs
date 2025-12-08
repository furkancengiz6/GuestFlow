using GuestFlow.Application.Operations.Import;
using GuestFlow.Application.Types;
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
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("İçe Aktarma")]
    public class ImportController : BaseController
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        /// <summary>
        /// Excel dosyasından misafir listesini içe aktarır (preview - kaydetmeden)
        /// </summary>
        /// <param name="file">Excel dosyası (.xlsx, .xls)</param>
        /// <returns>Önizleme verileri (kaydedilmeden)</returns>
        /// <response code="200">Dosya başarıyla okundu, önizleme verileri döndürüldü</response>
        /// <response code="400">Dosya seçilmedi veya geçersiz format</response>
        [HttpPost("guests/excel/preview")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> PreviewGuestsFromExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Error("Dosya seçilmedi.", 400);

                var result = await _importService.ImportGuestsFromExcelAsync(file);

                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dosya okunurken hata oluştu.", 400);

                return Success(new
                {
                    TotalRows = result.TotalRows,
                    ValidRows = result.ValidRows,
                    InvalidRows = result.InvalidRows,
                    Data = result.Data,
                    Errors = result.Errors
                }, "Dosya başarıyla okundu. Önizleme için veriler hazır.");
            }
            catch (Exception ex)
            {
                return Error($"Dosya okunurken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// CSV dosyasından misafir listesini içe aktarır (preview - kaydetmeden)
        /// </summary>
        /// <param name="file">CSV dosyası (.csv)</param>
        /// <returns>Önizleme verileri (kaydedilmeden)</returns>
        /// <response code="200">Dosya başarıyla okundu, önizleme verileri döndürüldü</response>
        /// <response code="400">Dosya seçilmedi veya geçersiz format</response>
        [HttpPost("guests/csv/preview")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> PreviewGuestsFromCsv(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Error("Dosya seçilmedi.", 400);

                var result = await _importService.ImportGuestsFromCsvAsync(file);

                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dosya okunurken hata oluştu.", 400);

                return Success(new
                {
                    TotalRows = result.TotalRows,
                    ValidRows = result.ValidRows,
                    InvalidRows = result.InvalidRows,
                    Data = result.Data,
                    Errors = result.Errors
                }, "Dosya başarıyla okundu. Önizleme için veriler hazır.");
            }
            catch (Exception ex)
            {
                return Error($"Dosya okunurken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Excel dosyasından misafir listesini içe aktarır ve kaydeder
        /// </summary>
        /// <param name="file">Excel dosyası (.xlsx, .xls)</param>
        /// <param name="skipDuplicates">Tekrarlanan kayıtları atla (varsayılan: true)</param>
        /// <returns>İçe aktarma sonucu</returns>
        /// <response code="200">Misafirler başarıyla içe aktarıldı</response>
        /// <response code="400">Dosya seçilmedi, geçersiz format veya içe aktarılacak geçerli kayıt yok</response>
        [HttpPost("guests/excel")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ImportGuestsFromExcel(
            IFormFile file,
            [FromForm] bool skipDuplicates = true)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Error("Dosya seçilmedi.", 400);

                // Önce dosyayı oku
                var importResult = await _importService.ImportGuestsFromExcelAsync(file);

                if (!importResult.IsSuccess)
                    return Error(importResult.ErrorMessage ?? "Dosya okunurken hata oluştu.", 400);

                // Sadece geçerli kayıtları kaydet
                var validGuests = importResult.Data.Where(g => g.IsValid).ToList();

                if (validGuests.Count == 0)
                    return Error("İçe aktarılacak geçerli kayıt bulunamadı.", 400);

                // Kaydet
                var saveResult = await _importService.SaveImportedGuestsAsync(validGuests, skipDuplicates);

                if (!saveResult.IsSuccess)
                    return Error(saveResult.Message, 400);

                return Success(saveResult.Data, saveResult.Message);
            }
            catch (Exception ex)
            {
                return Error($"İçe aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// CSV dosyasından misafir listesini içe aktarır ve kaydeder
        /// </summary>
        /// <param name="file">CSV dosyası (.csv)</param>
        /// <param name="skipDuplicates">Tekrarlanan kayıtları atla (varsayılan: true)</param>
        /// <returns>İçe aktarma sonucu</returns>
        /// <response code="200">Misafirler başarıyla içe aktarıldı</response>
        /// <response code="400">Dosya seçilmedi, geçersiz format veya içe aktarılacak geçerli kayıt yok</response>
        [HttpPost("guests/csv")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ImportGuestsFromCsv(
            IFormFile file,
            [FromForm] bool skipDuplicates = true)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Error("Dosya seçilmedi.", 400);

                // Önce dosyayı oku
                var importResult = await _importService.ImportGuestsFromCsvAsync(file);

                if (!importResult.IsSuccess)
                    return Error(importResult.ErrorMessage ?? "Dosya okunurken hata oluştu.", 400);

                // Sadece geçerli kayıtları kaydet
                var validGuests = importResult.Data.Where(g => g.IsValid).ToList();

                if (validGuests.Count == 0)
                    return Error("İçe aktarılacak geçerli kayıt bulunamadı.", 400);

                // Kaydet
                var saveResult = await _importService.SaveImportedGuestsAsync(validGuests, skipDuplicates);

                if (!saveResult.IsSuccess)
                    return Error(saveResult.Message, 400);

                return Success(saveResult.Data, saveResult.Message);
            }
            catch (Exception ex)
            {
                return Error($"İçe aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Önizleme sonrası seçilen misafirleri kaydeder
        /// </summary>
        [HttpPost("guests/save")]
        public async Task<IActionResult> SaveImportedGuests(
            [FromBody] SaveImportedGuestsRequest request)
        {
            try
            {
                if (request == null || request.Guests == null || request.Guests.Count == 0)
                    return Error("Kaydedilecek misafir bulunamadı.", 400);

                var saveResult = await _importService.SaveImportedGuestsAsync(request.Guests, request.SkipDuplicates);

                if (!saveResult.IsSuccess)
                    return Error(saveResult.Message, 400);

                return Success(saveResult.Data, saveResult.Message);
            }
            catch (Exception ex)
            {
                return Error($"Kayıt sırasında hata oluştu: {ex.Message}", 500);
            }
        }
    }

    /// <summary>
    /// İçe aktarılan misafirleri kaydetme isteği
    /// </summary>
    public class SaveImportedGuestsRequest
    {
        public List<ImportGuestDto> Guests { get; set; } = new List<ImportGuestDto>();
        public bool SkipDuplicates { get; set; } = true;
    }
}

