using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Import
{
    public class ImportService : IImportService
    {
        private readonly IGuestService _guestService;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ImportService> _logger;

        public ImportService(
            IGuestService guestService,
            IRepository<GuestEntity> guestRepository,
            IUnitOfWork unitOfWork,
            ILogger<ImportService> logger)
        {
            _guestService = guestService;
            _guestRepository = guestRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ImportResult<ImportGuestDto>> ImportGuestsFromExcelAsync(IFormFile file)
        {
            var result = new ImportResult<ImportGuestDto>();

            try
            {
                if (file == null || file.Length == 0)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Dosya seçilmedi veya dosya boş.";
                    return result;
                }

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
                    !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Geçersiz dosya formatı. Sadece Excel dosyaları (.xlsx, .xls) desteklenmektedir.";
                    return result;
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.First();

                // Başlık satırını bul
                int headerRow = 1;
                var headers = new Dictionary<string, int>();

                for (int col = 1; col <= worksheet.LastColumnUsed().ColumnNumber(); col++)
                {
                    var headerValue = worksheet.Cell(headerRow, col).GetString().Trim().ToLower();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        headers[headerValue] = col;
                    }
                }

                // Gerekli sütunları kontrol et
                if (!headers.ContainsKey("ad soyad") && !headers.ContainsKey("fullname") && !headers.ContainsKey("name"))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Excel dosyasında 'Ad Soyad', 'FullName' veya 'Name' sütunu bulunamadı.";
                    return result;
                }

                result.TotalRows = worksheet.LastRowUsed().RowNumber() - 1; // Başlık satırını çıkar

                // Verileri oku
                for (int row = 2; row <= worksheet.LastRowUsed().RowNumber(); row++)
                {
                    var guest = new ImportGuestDto { RowNumber = row };

                    try
                    {
                        // Ad Soyad
                        var nameCol = headers.ContainsKey("ad soyad") ? headers["ad soyad"] :
                                     headers.ContainsKey("fullname") ? headers["fullname"] :
                                     headers.ContainsKey("name") ? headers["name"] : 0;
                        if (nameCol > 0)
                            guest.FullName = worksheet.Cell(row, nameCol).GetString().Trim();

                        // Email
                        var emailCol = headers.ContainsKey("email") ? headers["email"] :
                                      headers.ContainsKey("e-posta") ? headers["e-posta"] : 0;
                        if (emailCol > 0)
                            guest.Email = worksheet.Cell(row, emailCol).GetString().Trim();

                        // Telefon
                        var phoneCol = headers.ContainsKey("telefon") ? headers["telefon"] :
                                      headers.ContainsKey("phone") ? headers["phone"] :
                                      headers.ContainsKey("phone number") ? headers["phone number"] : 0;
                        if (phoneCol > 0)
                            guest.PhoneNumber = worksheet.Cell(row, phoneCol).GetString().Trim();

                        // Uyruk
                        var nationalityCol = headers.ContainsKey("uyruk") ? headers["uyruk"] :
                                            headers.ContainsKey("nationality") ? headers["nationality"] : 0;
                        if (nationalityCol > 0)
                            guest.Nationality = worksheet.Cell(row, nationalityCol).GetString().Trim();

                        // Özel Misafir
                        var specialCol = headers.ContainsKey("özel misafir") ? headers["özel misafir"] :
                                        headers.ContainsKey("special guest") ? headers["special guest"] :
                                        headers.ContainsKey("is special") ? headers["is special"] : 0;
                        if (specialCol > 0)
                        {
                            var specialValue = worksheet.Cell(row, specialCol).GetString().Trim().ToLower();
                            guest.IsSpecialGuest = specialValue == "evet" || specialValue == "yes" || specialValue == "true" || specialValue == "1";
                        }

                        // Validasyon
                        ValidateGuest(guest);

                        if (guest.IsValid)
                            result.ValidRows++;
                        else
                            result.InvalidRows++;

                        result.Data.Add(guest);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = row,
                            Message = $"Satır okunurken hata: {ex.Message}",
                            RowData = $"Row {row}"
                        });
                        result.InvalidRows++;
                        guest.IsValid = false;
                        result.Data.Add(guest);
                    }
                }

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Excel dosyası okunurken hata: {ex.Message}");
                result.IsSuccess = false;
                result.ErrorMessage = $"Dosya okunurken hata oluştu: {ex.Message}";
                return result;
            }
        }

        public async Task<ImportResult<ImportGuestDto>> ImportGuestsFromCsvAsync(IFormFile file)
        {
            var result = new ImportResult<ImportGuestDto>();

            try
            {
                if (file == null || file.Length == 0)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Dosya seçilmedi veya dosya boş.";
                    return result;
                }

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Geçersiz dosya formatı. Sadece CSV dosyaları (.csv) desteklenmektedir.";
                    return result;
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var reader = new StreamReader(stream, Encoding.UTF8);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null
                };

                using var csv = new CsvReader(reader, config);

                // Başlıkları oku
                await csv.ReadAsync();
                csv.ReadHeader();
                var headers = csv.HeaderRecord?.Select(h => h.Trim().ToLower()).ToList() ?? new List<string>();

                if (!headers.Contains("ad soyad") && !headers.Contains("fullname") && !headers.Contains("name"))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "CSV dosyasında 'Ad Soyad', 'FullName' veya 'Name' sütunu bulunamadı.";
                    return result;
                }

                int rowNumber = 1; // Başlık satırı

                while (await csv.ReadAsync())
                {
                    rowNumber++;
                    var guest = new ImportGuestDto { RowNumber = rowNumber };

                    try
                    {
                        // Ad Soyad
                        guest.FullName = GetCsvValue(csv, headers, "ad soyad", "fullname", "name");

                        // Email
                        guest.Email = GetCsvValue(csv, headers, "email", "e-posta");

                        // Telefon
                        guest.PhoneNumber = GetCsvValue(csv, headers, "telefon", "phone", "phone number");

                        // Uyruk
                        guest.Nationality = GetCsvValue(csv, headers, "uyruk", "nationality");

                        // Özel Misafir
                        var specialValue = GetCsvValue(csv, headers, "özel misafir", "special guest", "is special");
                        if (!string.IsNullOrEmpty(specialValue))
                        {
                            var lowerValue = specialValue.ToLower();
                            guest.IsSpecialGuest = lowerValue == "evet" || lowerValue == "yes" || lowerValue == "true" || lowerValue == "1";
                        }

                        // Validasyon
                        ValidateGuest(guest);

                        if (guest.IsValid)
                            result.ValidRows++;
                        else
                            result.InvalidRows++;

                        result.Data.Add(guest);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = rowNumber,
                            Message = $"Satır okunurken hata: {ex.Message}",
                            RowData = string.Join(",", csv.Parser.Record ?? Array.Empty<string>())
                        });
                        result.InvalidRows++;
                        guest.IsValid = false;
                        result.Data.Add(guest);
                    }
                }

                result.TotalRows = rowNumber - 1; // Başlık satırını çıkar
                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CSV dosyası okunurken hata: {ex.Message}");
                result.IsSuccess = false;
                result.ErrorMessage = $"Dosya okunurken hata oluştu: {ex.Message}";
                return result;
            }
        }

        public async Task<ServiceMessage<ImportSummaryDto>> SaveImportedGuestsAsync(List<ImportGuestDto> guests, bool skipDuplicates = true)
        {
            var summary = new ImportSummaryDto
            {
                TotalRows = guests.Count
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                foreach (var importGuest in guests)
                {
                    if (!importGuest.IsValid)
                    {
                        summary.ErrorCount++;
                        summary.Errors.AddRange(importGuest.ValidationErrors.Select(e => new ImportError
                        {
                            RowNumber = importGuest.RowNumber,
                            Message = e,
                            Field = "Validation"
                        }));
                        continue;
                    }

                    try
                    {
                        // Duplicate kontrolü
                        if (skipDuplicates)
                        {
                            var existingGuest = await _guestRepository.GetAll()
                                .FirstOrDefaultAsync(g =>
                                    (!string.IsNullOrEmpty(importGuest.Email) && g.Email == importGuest.Email) ||
                                    (!string.IsNullOrEmpty(importGuest.PhoneNumber) && g.PhoneNumber == importGuest.PhoneNumber));

                            if (existingGuest != null)
                            {
                                summary.SkippedCount++;
                                continue;
                            }
                        }

                        // Misafir ekle
                        var addGuestDto = new AddGuestDto
                        {
                            FullName = importGuest.FullName ?? string.Empty,
                            Email = importGuest.Email,
                            PhoneNumber = importGuest.PhoneNumber,
                            Nationality = importGuest.Nationality,
                            IsSpecialGuest = importGuest.IsSpecialGuest
                        };

                        var result = await _guestService.AddGuest(addGuestDto);
                        if (result.IsSuccess)
                        {
                            summary.SuccessCount++;
                        }
                        else
                        {
                            summary.ErrorCount++;
                            summary.Errors.Add(new ImportError
                            {
                                RowNumber = importGuest.RowNumber,
                                Message = result.Message,
                                Field = "Save"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Misafir kaydedilirken hata (Satır {importGuest.RowNumber}): {ex.Message}");
                        summary.ErrorCount++;
                        summary.Errors.Add(new ImportError
                        {
                            RowNumber = importGuest.RowNumber,
                            Message = $"Kayıt hatası: {ex.Message}",
                            Field = "Save"
                        });
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ServiceMessage<ImportSummaryDto>
                {
                    IsSuccess = true,
                    Message = $"İçe aktarma tamamlandı. Başarılı: {summary.SuccessCount}, Hatalı: {summary.ErrorCount}, Atlandı: {summary.SkippedCount}",
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Toplu içe aktarma sırasında hata: {ex.Message}");
                return new ServiceMessage<ImportSummaryDto>
                {
                    IsSuccess = false,
                    Message = $"İçe aktarma sırasında hata oluştu: {ex.Message}",
                    Data = summary
                };
            }
        }

        #region Private Methods

        /// <summary>
        /// Misafir verilerini validasyon yapar
        /// </summary>
        private void ValidateGuest(ImportGuestDto guest)
        {
            guest.ValidationErrors.Clear();

            // Ad Soyad zorunlu
            if (string.IsNullOrWhiteSpace(guest.FullName))
            {
                guest.ValidationErrors.Add("Ad Soyad alanı zorunludur.");
                guest.IsValid = false;
            }
            else if (guest.FullName.Length > 200)
            {
                guest.ValidationErrors.Add("Ad Soyad en fazla 200 karakter olabilir.");
                guest.IsValid = false;
            }

            // Özel misafir değilse email ve telefon zorunlu
            if (!guest.IsSpecialGuest)
            {
                if (string.IsNullOrWhiteSpace(guest.Email))
                {
                    guest.ValidationErrors.Add("Email alanı zorunludur.");
                    guest.IsValid = false;
                }
                else if (!guest.Email.Contains("@") || guest.Email.Length < 5 || guest.Email.Length > 100)
                {
                    guest.ValidationErrors.Add("Geçerli bir e-posta adresi giriniz.");
                    guest.IsValid = false;
                }

                if (string.IsNullOrWhiteSpace(guest.PhoneNumber))
                {
                    guest.ValidationErrors.Add("Telefon numarası alanı zorunludur.");
                    guest.IsValid = false;
                }
            }

            // Email format kontrolü (varsa)
            if (!string.IsNullOrWhiteSpace(guest.Email) && !guest.Email.Contains("@"))
            {
                guest.ValidationErrors.Add("Geçerli bir e-posta adresi giriniz.");
                guest.IsValid = false;
            }
        }

        /// <summary>
        /// CSV'den değer okur (birden fazla sütun adı denemesi yapar)
        /// </summary>
        private string? GetCsvValue(CsvReader csv, List<string> headers, params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                var index = headers.IndexOf(columnName.ToLower());
                if (index >= 0)
                {
                    try
                    {
                        return csv.GetField(index)?.Trim();
                    }
                    catch
                    {
                        // Sütun bulundu ama değer okunamadı
                    }
                }
            }
            return null;
        }

        #endregion
    }
}

