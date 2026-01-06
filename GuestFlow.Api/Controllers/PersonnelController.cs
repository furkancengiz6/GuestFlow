using GuestFlow.Api.Models;
using GuestFlow.Api.Models.PersonnelModels;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Personnel;
using GuestFlow.Application.Operations.Personnel.Dtos;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Personel yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Owner")] // Sadece Owner ve Admin personel yönetimi yapabilir
    [Tags("Personeller")]
    public class PersonnelController : BaseController
    {
        private readonly IPersonnelService _personnelService;

        public PersonnelController(IPersonnelService personnelService)
        {
            _personnelService = personnelService;
        }

        /// <summary>
        /// Tüm personelleri listeler (sayfalanmış, filtrelenmiş ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="searchTerm">Arama terimi</param>
        /// <param name="userType">Kullanıcı tipi filtresi</param>
        /// <param name="startDate">Başlangıç tarihi filtresi</param>
        /// <param name="endDate">Bitiş tarihi filtresi</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: asc)</param>
        /// <returns>Sayfalanmış personel listesi</returns>
        /// <response code="200">Personel listesi başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<PersonnelInfoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllPersonnel(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? userType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc")
        {
            try
            {
                var filters = new PersonnelFilterParameters
                {
                    SearchTerm = searchTerm,
                    UserType = userType,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var sorting = new SortingParameters
                {
                    SortBy = sortBy,
                    SortOrder = sortOrder
                };

                var result = await _personnelService.GetPersonnelPagedAsync(pageNumber, pageSize, filters, sorting);
                return PagedResult<PersonnelInfoDto>(result, "Personeller başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Personel listesi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir personelin bilgilerini getirir (temel bilgiler)
        /// </summary>
        /// <param name="id">Personel ID'si</param>
        /// <returns>Personel bilgileri</returns>
        /// <response code="200">Personel bilgisi başarıyla getirildi</response>
        /// <response code="404">Personel bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPersonnelById(int id)
        {
            try
            {
                var result = await _personnelService.GetPersonnelById(id);
                if (result.IsSuccess)
                {
                    return Success(result.Data, "Personel bilgisi başarıyla getirildi.");
                }
                return Error(result.Message, 404);
            }
            catch (Exception ex)
            {
                return Error("Personel bilgisi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Personel detayını getirir (ilgili veriler ile - istatistikler, aktiviteler)
        /// </summary>
        /// <param name="id">Personel ID'si</param>
        /// <returns>Personel detay bilgileri</returns>
        /// <response code="200">Personel detayı başarıyla getirildi</response>
        /// <response code="404">Personel bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPersonnelDetail(int id)
        {
            try
            {
                var result = await _personnelService.GetPersonnelDetailAsync(id);
                return Success(result, "Personel detayı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Personel detayı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Personel aktivite günlüklerini getirir
        /// </summary>
        /// <param name="id">Personel ID'si</param>
        /// <param name="limit">Kayıt limiti (varsayılan: 20)</param>
        /// <returns>Personel aktiviteleri listesi</returns>
        /// <response code="200">Personel aktiviteleri başarıyla getirildi</response>
        /// <response code="404">Personel bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/activities")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPersonnelActivities(int id, [FromQuery] int? limit = 20)
        {
            try
            {
                var result = await _personnelService.GetPersonnelActivitiesAsync(id, limit);
                return Success(result, "Personel aktiviteleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Personel aktiviteleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Yeni personel ekler
        /// </summary>
        /// <param name="request">Personel bilgileri</param>
        /// <returns>Oluşturulan personel bilgileri</returns>
        /// <response code="200">Personel başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PersonnelInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddPersonnel([FromBody] AddPersonnelRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var addPersonnelDto = new AddPersonnelDto
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    Password = request.Password
                };

                var result = await _personnelService.AddPersonnel(addPersonnelDto);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Personel eklenirken bir hata oluştu.", Error = ex.Message });
            }
        }

        /// <summary>
        /// Personel bilgilerini günceller
        /// </summary>
        /// <param name="id">Personel ID'si</param>
        /// <param name="request">Güncellenecek personel bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Personel başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Personel bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePersonnel(int id, [FromBody] UpdatePersonnelRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != request.Id)
                {
                    return BadRequest(new { Message = "URL'deki ID ile body'deki ID eşleşmiyor." });
                }

                var updatePersonnelDto = new UpdatePersonnelDto
                {
                    Id = request.Id,
                    FullName = request.FullName,
                    Email = request.Email,
                    UserType = request.UserType,
                    NewPassword = request.NewPassword
                };

                var result = await _personnelService.UpdatePersonnel(updatePersonnelDto);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Personel güncellenirken bir hata oluştu.", Error = ex.Message });
            }
        }

        /// <summary>
        /// Personeli siler (soft delete)
        /// </summary>
        /// <param name="id">Personel ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Personel başarıyla silindi</response>
        /// <response code="400">Kendi hesabınızı silemezsiniz</response>
        /// <response code="404">Personel bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePersonnel(int id)
        {
            try
            {
                // Kendi hesabını silmeyi engelle
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    if (id == currentUserId)
                    {
                        return BadRequest(new { Message = "Kendi hesabınızı silemezsiniz." });
                    }
                }

                var result = await _personnelService.DeletePersonnel(id);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Personel silinirken bir hata oluştu.", Error = ex.Message });
            }
        }

        /// <summary>
        /// Personel rolünü değiştirir (Sadece Admin)
        /// </summary>
        /// <param name="id">Personel ID'si</param>
        /// <param name="request">Yeni rol bilgisi</param>
        /// <returns>Rol değiştirme sonucu</returns>
        /// <response code="200">Personel rolü başarıyla değiştirildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Personel bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPatch("{id}/role")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePersonnelRole(int id, [FromBody] ChangeRoleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kendi rolünü değiştirmeyi engelle
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    if (id == currentUserId)
                    {
                        return BadRequest(new { Message = "Kendi rolünüzü değiştiremezsiniz." });
                    }
                }

                // Personeli bul
                var personnelResult = await _personnelService.GetPersonnelById(id);
                if (!personnelResult.IsSuccess || personnelResult.Data == null)
                {
                    return Error("Personel bulunamadı.", 404);
                }

                // UpdatePersonnel ile rolü güncelle
                var updatePersonnelDto = new UpdatePersonnelDto
                {
                    Id = id,
                    FullName = personnelResult.Data.FullName,
                    Email = personnelResult.Data.Email,
                    UserType = request.UserType
                };

                var result = await _personnelService.UpdatePersonnel(updatePersonnelDto);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Personel rolü değiştirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}

