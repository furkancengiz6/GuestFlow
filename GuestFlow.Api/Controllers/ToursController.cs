using GuestFlow.Application.Operations.Tour;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    public class ToursController : BaseController
    {
        private readonly ITourService _tourService;

        public ToursController(ITourService tourService)
        {
            _tourService = tourService;
        }

        /// <summary>
        /// Tur tanımlarını döner (şehir ve aktiflik filtresi)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTours([FromQuery] int? cityId = null, [FromQuery] bool? isActive = true)
        {
            var result = await _tourService.GetToursAsync(cityId, isActive);
            return Success(result, "Turlar başarıyla getirildi.");
        }

        /// <summary>
        /// Tur takvim görünümünü getirir (CityTour ve YachtTour birleşik)
        /// </summary>
        [HttpGet("calendar")]
        public async Task<IActionResult> GetTourCalendar(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _tourService.GetTourCalendarAsync(startDate, endDate);
                return Success(result, "Tur takvimi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Tur takvimi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tur istatistiklerini getirir (CityTour ve YachtTour birleşik)
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetTourStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _tourService.GetTourStatisticsAsync(startDate, endDate);
                return Success(result, "Tur istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Tur istatistikleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}

