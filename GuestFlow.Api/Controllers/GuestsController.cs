using GuestFlow.Api.Models;
using GuestFlow.Api.Models.GuestModels;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Misafir yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    [Tags("Misafirler")]
    public class GuestsController : BaseController
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _guestService: Misafirlerle ilgili işlemleri yapmak için kullanıyorum.
        private readonly IGuestService _guestService;
        private readonly IRoomAssignmentService _roomAssignmentService;
        private readonly IGuestPreferencesService _guestPreferencesService;
        private readonly IGuestPreferenceAnalysisService _preferenceAnalysisService;
        private readonly ILogger<GuestsController> _logger;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public GuestsController(
            IGuestService guestService, 
            IRoomAssignmentService roomAssignmentService, 
            IGuestPreferencesService guestPreferencesService,
            IGuestPreferenceAnalysisService preferenceAnalysisService,
            ILogger<GuestsController> logger)
        {
            _guestService = guestService;
            _roomAssignmentService = roomAssignmentService;
            _guestPreferencesService = guestPreferencesService;
            _preferenceAnalysisService = preferenceAnalysisService;
            _logger = logger;
        }

        /// <summary>
        /// Yeni bir misafir ekler
        /// </summary>
        /// <param name="request">Misafir bilgileri</param>
        /// <returns>Oluşturulan misafir bilgileri</returns>
        /// <response code="200">Misafir başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <example>
        /// <code>
        /// POST /api/v1/guests
        /// {
        ///   "fullName": "John Doe",
        ///   "email": "john.doe@example.com",
        ///   "phoneNumber": "+90 555 123 4567",
        ///   "nationality": "US",
        ///   "isSpecialGuest": false
        /// }
        /// </code>
        /// </example>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetGuestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddGuest(AddGuestRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var addGuestDto = new AddGuestDto
            {
                FullName = request.FullName,
                Email = request.Email,
                Nationality = request.Nationality,
                PhoneNumber = request.PhoneNumber,
                IsSpecialGuest = request.IsSpecialGuest
            };

            // Misafiri eklemek için servisi çağırıyorum.
            var result = await _guestService.AddGuest(addGuestDto);
            // Standart API yanıt formatını kullan
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Tüm misafirleri getirir (sayfalanmış, filtrelenmiş ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="searchTerm">Arama terimi (isim, email, telefon)</param>
        /// <param name="nationality">Uyruk filtresi</param>
        /// <param name="isSpecialGuest">Özel misafir filtresi</param>
        /// <param name="email">Email filtresi</param>
        /// <param name="phoneNumber">Telefon numarası filtresi</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: asc)</param>
        /// <returns>Sayfalanmış misafir listesi</returns>
        /// <response code="200">Misafir listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<GetGuestDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuests(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? nationality = null,
            [FromQuery] bool? isSpecialGuest = null,
            [FromQuery] string? email = null,
            [FromQuery] string? phoneNumber = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc")
        {
            // Filtreleme parametrelerini oluştur
            var filters = new GuestFlow.Application.Models.GuestFilterParameters
            {
                SearchTerm = searchTerm,
                Nationality = nationality,
                IsSpecialGuest = isSpecialGuest,
                Email = email,
                PhoneNumber = phoneNumber
            };

            // Sıralama parametrelerini oluştur
            var sorting = new GuestFlow.Application.Models.SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Servisten sayfalanmış, filtrelenmiş ve sıralanmış misafirleri alıyorum ve JSON formatında döndürüyorum.
            var result = await _guestService.GetGuestsPaged(pageNumber, pageSize, filters, sorting);
            return PagedResult<GetGuestDto>(result, "Misafirler başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir misafiri ID'sine göre getirir
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Misafir bilgileri</returns>
        /// <response code="200">Misafir başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetGuestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestById(int id)
        {
            // Servisten misafiri ID'sine göre alıyorum.
            var result = await _guestService.GetGuestById(id);
            // Standart API yanıt formatını kullan
            return result == null ? NotFound("Misafir bulunamadı.") : Success(result);
        }

        /// <summary>
        /// Mevcut bir misafiri günceller
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <param name="request">Güncellenecek misafir bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Misafir başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateGuestRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateGuestDto = new UpdateGuestDto
            {
                Id = id,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Nationality = request.Nationality,
                IsSpecialGuest = request.IsSpecialGuest
            };

            // Misafiri güncellemek için servisi çağırıyorum.
            var result = await _guestService.UpdateGuest(updateGuestDto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bir misafiri siler (soft delete)
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Misafir başarıyla silindi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            // Misafiri silmek için servisi çağırıyorum.
            var result = await _guestService.DeleteGuest(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Misafir detayını getirir (geçmiş ile - transferler, turlar, faturalar)
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Misafir detay bilgileri</returns>
        /// <response code="200">Misafir detayı başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestDetail(int id)
        {
            try
            {
                var result = await _guestService.GetGuestDetailAsync(id);
                return Success(result, "Misafir detayı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir detayı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafir faturalarını getirir
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Misafir faturaları listesi</returns>
        /// <response code="200">Faturalar başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/invoices")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestInvoices(int id)
        {
            try
            {
                var result = await _guestService.GetGuestInvoicesAsync(id);
                return Success(result, "Misafir faturaları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir faturaları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafir zaman çizelgesini getirir (transferler, turlar kronolojik sırada)
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Misafir zaman çizelgesi</returns>
        /// <response code="200">Zaman çizelgesi başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/timeline")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestTimeline(int id)
        {
            try
            {
                var result = await _guestService.GetGuestTimelineAsync(id);
                return Success(result, "Misafir zaman çizelgesi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir zaman çizelgesi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        private int GetCurrentPersonnelId()
        {
            var personnelIdClaim = User.FindFirst("PersonnelId");
            if (personnelIdClaim != null && int.TryParse(personnelIdClaim.Value, out var personnelId))
            {
                return personnelId;
            }
            return 1; // Default to admin if not found
        }

        // ================================
        // ROOM ASSIGNMENT ENDPOINTS
        // ================================

        /// <summary>
        /// Misafir için yeni oda ataması oluşturur
        /// </summary>
        [HttpPost("{guestId}/room-assignments")]
        [ProducesResponseType(typeof(ApiResponse<RoomAssignmentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateRoomAssignment(int guestId, [FromBody] CreateRoomAssignmentRequest request)
        {
            try
            {
                if (guestId != request.GuestId)
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "URL'deki misafir ID ile istekteki misafir ID eşleşmiyor." });

                var currentPersonnelId = GetCurrentPersonnelId();

                var dto = new CreateRoomAssignmentDto
                {
                    GuestId = request.GuestId,
                    HotelId = request.HotelId,
                    RoomNumber = request.RoomNumber,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Notes = request.Notes,
                    PersonnelId = currentPersonnelId
                };

                var result = await _roomAssignmentService.CreateRoomAssignmentAsync(dto);

                if (!result.IsSuccess)
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
                }

                return StatusCode(StatusCodes.Status201Created, Success(result.Data, "Oda ataması başarıyla oluşturuldu."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Oda ataması oluşturulurken hata: {ex.Message}");
                return Error("Oda ataması oluşturulurken hata oluştu.", 500);
            }
        }

        /// <summary>
        /// Misafirin oda atamalarını getirir
        /// </summary>
        [HttpGet("{guestId}/room-assignments")]
        [ProducesResponseType(typeof(ApiResponse<List<RoomAssignmentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestRoomAssignments(int guestId)
        {
            try
            {
                var result = await _roomAssignmentService.GetGuestRoomAssignmentsAsync(guestId);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir oda atamaları getirilirken hata: {ex.Message}");
                return Error("Misafir oda atamaları getirilirken hata oluştu.", 500);
            }
        }

        /// <summary>
        /// Misafirin mevcut oda atamasını getirir
        /// </summary>
        [HttpGet("{guestId}/current-room")]
        [ProducesResponseType(typeof(ApiResponse<RoomAssignmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentRoomAssignment(int guestId)
        {
            try
            {
                var result = await _roomAssignmentService.GetCurrentRoomAssignmentAsync(guestId);

                if (!result.IsSuccess)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = result.Message });
                }

                return Success(result.Data, "Aktif oda ataması başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Aktif oda ataması getirilirken hata: {ex.Message}");
                return Error("Aktif oda ataması getirilirken hata oluştu.", 500);
            }
        }

        /// <summary>
        /// Misafir tercihlerini getirir
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Misafir tercihleri</returns>
        /// <response code="200">Tercihler başarıyla getirildi</response>
        /// <response code="404">Tercihler bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/preferences")]
        [ProducesResponseType(typeof(ApiResponse<GuestPreferencesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestPreferences(int id)
        {
            var result = await _guestPreferencesService.GetGuestPreferencesAsync(id);
            if (result.Success)
                return Success(result.Data, result.Message);
            return BadRequest(result);
        }

        /// <summary>
        /// Misafir tercihlerini oluşturur veya günceller
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <param name="dto">Tercih bilgileri</param>
        /// <returns>Oluşturulan/güncellenen tercihler</returns>
        /// <response code="200">Tercihler başarıyla kaydedildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}/preferences")]
        [ProducesResponseType(typeof(ApiResponse<GuestPreferencesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpsertGuestPreferences(int id, [FromBody] UpsertGuestPreferencesDto dto)
        {
            dto.GuestId = id; // Ensure guest ID matches route parameter
            var result = await _guestPreferencesService.UpsertGuestPreferencesAsync(id, dto);
            if (result.Success)
                return Success(result.Data, result.Message);
            return BadRequest(result);
        }

        /// <summary>
        /// Misafir tercihlerini siler
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Silme işlemi sonucu</returns>
        /// <response code="200">Tercihler başarıyla silindi</response>
        /// <response code="404">Tercihler bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}/preferences")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteGuestPreferences(int id)
        {
            var result = await _guestPreferencesService.DeleteGuestPreferencesAsync(id);
            if (result.Success)
                return Success(result.Data, result.Message);
            return BadRequest(result);
        }

        /// <summary>
        /// Misafir tercih analizini getirir
        /// </summary>
        [HttpGet("{id}/preferences/analysis")]
        [ProducesResponseType(typeof(ApiResponse<GuestPreferenceAnalysisDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGuestPreferenceAnalysis(int id)
        {
            try
            {
                var result = await _preferenceAnalysisService.GetPreferenceAnalysisAsync(id);
                return result.Success ? Success(result.Data, "Preference analysis retrieved successfully") : Error(result.Message, 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get preference analysis for guest {GuestId}", id);
                return Error("Failed to get preference analysis", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafir tercih önerilerini getirir (Intelligence Layer'dan)
        /// </summary>
        [HttpGet("{id}/preferences/recommendations")]
        [ProducesResponseType(typeof(ApiResponse<List<PreferenceRecommendationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGuestPreferenceRecommendations(int id)
        {
            try
            {
                var result = await _preferenceAnalysisService.GetPreferenceRecommendationsAsync(id);
                return result.Success ? Success(result.Data, "Preference recommendations retrieved successfully") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get preference recommendations for guest {GuestId}", id);
                return Error("Failed to get preference recommendations", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// PMS'den gelen tercihleri GuestFlow tercihleri ile birleştirir
        /// </summary>
        [HttpPost("{id}/preferences/merge-from-pms")]
        [ProducesResponseType(typeof(ApiResponse<GuestPreferencesDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MergePreferencesFromPMS(int id, [FromQuery] int pmsIntegrationId)
        {
            try
            {
                var result = await _preferenceAnalysisService.MergePreferencesFromPMSAsync(id, pmsIntegrationId);
                return result.Success ? Success(result.Data, "Preferences merged from PMS successfully") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to merge preferences from PMS for guest {GuestId}", id);
                return Error("Failed to merge preferences from PMS", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tercih uyumluluğunu hesaplar (misafir tercihleri ile hizmet arasında)
        /// </summary>
        [HttpGet("{id}/preferences/compatibility")]
        [ProducesResponseType(typeof(ApiResponse<PreferenceCompatibilityDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CalculatePreferenceCompatibility(int id, [FromQuery] string serviceType, [FromQuery] int? serviceId = null)
        {
            try
            {
                var result = await _preferenceAnalysisService.CalculatePreferenceCompatibilityAsync(id, serviceType, serviceId);
                return result.Success ? Success(result.Data, "Preference compatibility calculated successfully") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate preference compatibility for guest {GuestId}", id);
                return Error("Failed to calculate preference compatibility", 500, new { Error = ex.Message });
            }
        }
    }
}