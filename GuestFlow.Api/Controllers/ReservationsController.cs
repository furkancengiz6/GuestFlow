using GuestFlow.Api.Models;
using GuestFlow.Api.Models.ReservationModels;
using GuestFlow.Application.Operations.Reservation;
using GuestFlow.Application.Operations.Reservation.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Rezervasyon yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("Rezervasyonlar")]
    public class ReservationsController : BaseController
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>
        /// Yeni rezervasyon oluşturur
        /// </summary>
        /// <param name="request">Rezervasyon bilgileri</param>
        /// <returns>Oluşturulan rezervasyon bilgileri</returns>
        /// <response code="200">Rezervasyon başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetReservationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateReservation(AddReservationRequest request)
        {
            var dto = new AddReservationDto
            {
                GuestId = request.GuestId,
                PersonnelId = request.PersonnelId,
                ServiceType = request.ServiceType,
                ServiceId = request.ServiceId,
                ReservationDate = request.ReservationDate,
                Notes = request.Notes
            };

            var result = await _reservationService.CreateReservationAsync(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Rezervasyonu onaylar
        /// </summary>
        /// <param name="id">Rezervasyon ID'si</param>
        /// <returns>Onaylama sonucu</returns>
        /// <response code="200">Rezervasyon başarıyla onaylandı</response>
        /// <response code="404">Rezervasyon bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("{id}/confirm")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            var result = await _reservationService.ConfirmReservationAsync(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Rezervasyonu iptal eder
        /// </summary>
        /// <param name="id">Rezervasyon ID'si</param>
        /// <param name="request">İptal nedeni (opsiyonel)</param>
        /// <returns>İptal sonucu</returns>
        /// <response code="200">Rezervasyon başarıyla iptal edildi</response>
        /// <response code="404">Rezervasyon bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelReservation(int id, [FromBody] CancelReservationRequest? request = null)
        {
            var cancellationReason = request?.CancellationReason;
            var result = await _reservationService.CancelReservationAsync(id, cancellationReason);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Rezervasyonu günceller
        /// </summary>
        /// <param name="id">Rezervasyon ID'si</param>
        /// <param name="request">Güncellenecek rezervasyon bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Rezervasyon başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Rezervasyon bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateReservation(int id, UpdateReservationRequest request)
        {
            var dto = new UpdateReservationDto
            {
                Id = id,
                Notes = request.Notes,
                ReservationDate = request.ReservationDate
            };

            var result = await _reservationService.UpdateReservationAsync(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Rezervasyonu ID'ye göre getirir
        /// </summary>
        /// <param name="id">Rezervasyon ID'si</param>
        /// <returns>Rezervasyon bilgileri</returns>
        /// <response code="200">Rezervasyon başarıyla getirildi</response>
        /// <response code="404">Rezervasyon bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetReservationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservationById(int id)
        {
            try
            {
                var result = await _reservationService.GetReservationByIdAsync(id);
                if (result == null)
                    return NotFound("Rezervasyon bulunamadı.");

                return Success(result, "Rezervasyon başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Rezervasyon getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Rezervasyon detayını getirir
        /// </summary>
        /// <param name="id">Rezervasyon ID'si</param>
        /// <returns>Rezervasyon detay bilgileri</returns>
        /// <response code="200">Rezervasyon detayı başarıyla getirildi</response>
        /// <response code="404">Rezervasyon bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservationDetail(int id)
        {
            try
            {
                var result = await _reservationService.GetReservationDetailAsync(id);
                if (result == null)
                    return NotFound("Rezervasyon bulunamadı.");

                return Success(result, "Rezervasyon detayı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Rezervasyon detayı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tüm rezervasyonları getirir (sayfalanmış, filtrelenmiş ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="startDate">Başlangıç tarihi filtresi</param>
        /// <param name="endDate">Bitiş tarihi filtresi</param>
        /// <param name="guestId">Misafir ID filtresi</param>
        /// <param name="personnelId">Personel ID filtresi</param>
        /// <param name="status">Durum filtresi</param>
        /// <param name="serviceType">Servis tipi filtresi</param>
        /// <param name="serviceId">Servis ID filtresi</param>
        /// <param name="searchTerm">Arama terimi</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: desc)</param>
        /// <returns>Sayfalanmış rezervasyon listesi</returns>
        /// <response code="200">Rezervasyon listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetReservationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? guestId = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? serviceType = null,
            [FromQuery] int? serviceId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc")
        {
            // Filtreleme parametrelerini oluştur
            var filters = new GuestFlow.Application.Models.ReservationFilterParameters
            {
                StartDate = startDate,
                EndDate = endDate,
                GuestId = guestId,
                PersonnelId = personnelId,
                Status = status,
                ServiceType = serviceType,
                ServiceId = serviceId,
                SearchTerm = searchTerm
            };

            // Sıralama parametrelerini oluştur
            var sorting = new GuestFlow.Application.Models.SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Servisten sayfalanmış, filtrelenmiş ve sıralanmış rezervasyonları alıyorum ve JSON formatında döndürüyorum.
            var result = await _reservationService.GetReservationsPagedAsync(pageNumber, pageSize, filters, sorting);
            return PagedResult<GetReservationDto>(result, "Rezervasyonlar başarıyla getirildi.");
        }

        /// <summary>
        /// Misafire ait rezervasyonları getirir
        /// </summary>
        /// <param name="guestId">Misafir ID'si</param>
        /// <returns>Misafir rezervasyonları listesi</returns>
        /// <response code="200">Misafir rezervasyonları başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-guest/{guestId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservationsByGuestId(int guestId)
        {
            try
            {
                var result = await _reservationService.GetReservationsByGuestIdAsync(guestId);
                return Success(result, "Misafir rezervasyonları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir rezervasyonları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Personel'e ait rezervasyonları getirir
        /// </summary>
        /// <param name="personnelId">Personel ID'si</param>
        /// <returns>Personel rezervasyonları listesi</returns>
        /// <response code="200">Personel rezervasyonları başarıyla getirildi</response>
        /// <response code="404">Personel bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-personnel/{personnelId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservationsByPersonnelId(int personnelId)
        {
            try
            {
                var result = await _reservationService.GetReservationsByPersonnelIdAsync(personnelId);
                return Success(result, "Personel rezervasyonları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Personel rezervasyonları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tarih aralığına göre rezervasyonları getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Tarih aralığına göre rezervasyonlar listesi</returns>
        /// <response code="200">Rezervasyonlar başarıyla getirildi</response>
        /// <response code="400">Geçersiz tarih aralığı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-date-range")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservationsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _reservationService.GetReservationsByDateRangeAsync(startDate, endDate);
                return Success(result, "Tarih aralığına göre rezervasyonlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Tarih aralığına göre rezervasyonlar getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Duruma göre rezervasyonları getirir
        /// </summary>
        /// <param name="status">Rezervasyon durumu</param>
        /// <returns>Duruma göre rezervasyonlar listesi</returns>
        /// <response code="200">Rezervasyonlar başarıyla getirildi</response>
        /// <response code="400">Geçersiz durum</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservationsByStatus(string status)
        {
            try
            {
                var result = await _reservationService.GetReservationsByStatusAsync(status);
                return Success(result, "Duruma göre rezervasyonlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Duruma göre rezervasyonlar getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}

