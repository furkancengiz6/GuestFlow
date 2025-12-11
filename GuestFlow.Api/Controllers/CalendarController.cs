using GuestFlow.Application.Operations.Calendar;
using Microsoft.AspNetCore.Authorization;
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
    public class CalendarController : BaseController
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        /// <summary>
        /// Transfer için iCal/ICS formatında takvim dosyası oluşturur
        /// </summary>
        [HttpGet("transfer/{transferId}")]
        public async Task<IActionResult> GetTransferCalendar(int transferId)
        {
            try
            {
                var result = await _calendarService.GenerateTransferCalendarAsync(transferId);

                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Takvim dosyası oluşturulamadı.", 400);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Takvim dosyası oluşturulurken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Şehir turu için iCal/ICS formatında takvim dosyası oluşturur
        /// </summary>
        [HttpGet("citytour/{cityTourId}")]
        public async Task<IActionResult> GetCityTourCalendar(int cityTourId)
        {
            try
            {
                var result = await _calendarService.GenerateCityTourCalendarAsync(cityTourId);

                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Takvim dosyası oluşturulamadı.", 400);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Takvim dosyası oluşturulurken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Yat turu için iCal/ICS formatında takvim dosyası oluşturur
        /// </summary>
        [HttpGet("yachttour/{yachtTourId}")]
        public async Task<IActionResult> GetYachtTourCalendar(int yachtTourId)
        {
            try
            {
                var result = await _calendarService.GenerateYachtTourCalendarAsync(yachtTourId);

                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Takvim dosyası oluşturulamadı.", 400);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Takvim dosyası oluşturulurken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Rezervasyon için iCal/ICS formatında takvim dosyası oluşturur
        /// </summary>
        [HttpGet("reservation/{reservationId}")]
        public async Task<IActionResult> GetReservationCalendar(int reservationId)
        {
            try
            {
                var result = await _calendarService.GenerateReservationCalendarAsync(reservationId);

                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Takvim dosyası oluşturulamadı.", 400);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Takvim dosyası oluşturulurken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Birden fazla transfer için toplu takvim dosyası oluşturur
        /// </summary>
        [HttpPost("transfers/bulk")]
        public async Task<IActionResult> GetBulkTransferCalendar(
            [FromBody] BulkTransferCalendarRequest request)
        {
            try
            {
                var transferIds = request.TransferIds ?? new List<int>();

                var result = await _calendarService.GenerateBulkTransferCalendarAsync(
                    transferIds,
                    request.StartDate,
                    request.EndDate);

                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Takvim dosyası oluşturulamadı.", 400);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Takvim dosyası oluşturulurken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Birden fazla tur için toplu takvim dosyası oluşturur
        /// </summary>
        [HttpPost("tours/bulk")]
        public async Task<IActionResult> GetBulkTourCalendar(
            [FromBody] BulkTourCalendarRequest request)
        {
            try
            {
                var cityTourIds = request.CityTourIds ?? new List<int>();
                var yachtTourIds = request.YachtTourIds ?? new List<int>();

                var result = await _calendarService.GenerateBulkTourCalendarAsync(
                    cityTourIds,
                    yachtTourIds,
                    request.StartDate,
                    request.EndDate);

                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Takvim dosyası oluşturulamadı.", 400);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Takvim dosyası oluşturulurken hata oluştu: {ex.Message}", 500);
            }
        }
    }

    /// <summary>
    /// Toplu transfer takvim isteği
    /// </summary>
    public class BulkTransferCalendarRequest
    {
        public List<int>? TransferIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Toplu tur takvim isteği
    /// </summary>
    public class BulkTourCalendarRequest
    {
        public List<int>? CityTourIds { get; set; }
        public List<int>? YachtTourIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

