using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class ExportController : BaseController
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        /// <summary>
        /// Misafir listesini Excel formatında dışa aktarır
        /// </summary>
        [HttpGet("guests/excel")]
        public async Task<IActionResult> ExportGuestsToExcel(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? nationality = null,
            [FromQuery] bool? isSpecialGuest = null,
            [FromQuery] string? email = null,
            [FromQuery] string? phoneNumber = null)
        {
            try
            {
                var filters = new GuestFilterParameters
                {
                    SearchTerm = searchTerm,
                    Nationality = nationality,
                    IsSpecialGuest = isSpecialGuest,
                    Email = email,
                    PhoneNumber = phoneNumber
                };

                var result = await _exportService.ExportGuestsToExcelAsync(filters);
                
                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dışa aktarma başarısız oldu.", 500);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Dışa aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Misafir listesini CSV formatında dışa aktarır
        /// </summary>
        [HttpGet("guests/csv")]
        public async Task<IActionResult> ExportGuestsToCsv(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? nationality = null,
            [FromQuery] bool? isSpecialGuest = null,
            [FromQuery] string? email = null,
            [FromQuery] string? phoneNumber = null)
        {
            try
            {
                var filters = new GuestFilterParameters
                {
                    SearchTerm = searchTerm,
                    Nationality = nationality,
                    IsSpecialGuest = isSpecialGuest,
                    Email = email,
                    PhoneNumber = phoneNumber
                };

                var result = await _exportService.ExportGuestsToCsvAsync(filters);
                
                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dışa aktarma başarısız oldu.", 500);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Dışa aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Fatura listesini Excel formatında dışa aktarır
        /// </summary>
        [HttpGet("invoices/excel")]
        public async Task<IActionResult> ExportInvoicesToExcel(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? guestId = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] string? currency = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] string? serviceType = null,
            [FromQuery] int? serviceId = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var filters = new InvoiceFilterParameters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    GuestId = guestId,
                    MinAmount = minAmount,
                    MaxAmount = maxAmount,
                    Currency = currency,
                    PersonnelId = personnelId,
                    ServiceType = serviceType,
                    ServiceId = serviceId,
                    SearchTerm = searchTerm
                };

                var result = await _exportService.ExportInvoicesToExcelAsync(filters);
                
                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dışa aktarma başarısız oldu.", 500);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Dışa aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Fatura listesini CSV formatında dışa aktarır
        /// </summary>
        [HttpGet("invoices/csv")]
        public async Task<IActionResult> ExportInvoicesToCsv(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? guestId = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] string? currency = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] string? serviceType = null,
            [FromQuery] int? serviceId = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var filters = new InvoiceFilterParameters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    GuestId = guestId,
                    MinAmount = minAmount,
                    MaxAmount = maxAmount,
                    Currency = currency,
                    PersonnelId = personnelId,
                    ServiceType = serviceType,
                    ServiceId = serviceId,
                    SearchTerm = searchTerm
                };

                var result = await _exportService.ExportInvoicesToCsvAsync(filters);
                
                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dışa aktarma başarısız oldu.", 500);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Dışa aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Gelir raporunu Excel formatında dışa aktarır
        /// </summary>
        [HttpGet("revenue/excel")]
        public async Task<IActionResult> ExportRevenueReportToExcel(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _exportService.ExportRevenueReportToExcelAsync(startDate, endDate);
                
                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dışa aktarma başarısız oldu.", 500);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Dışa aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Gelir raporunu CSV formatında dışa aktarır
        /// </summary>
        [HttpGet("revenue/csv")]
        public async Task<IActionResult> ExportRevenueReportToCsv(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _exportService.ExportRevenueReportToCsvAsync(startDate, endDate);
                
                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dışa aktarma başarısız oldu.", 500);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Dışa aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Transfer listesini Excel formatında dışa aktarır
        /// </summary>
        [HttpGet("transfers/excel")]
        public async Task<IActionResult> ExportTransfersToExcel(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null,
            [FromQuery] int? guestId = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] int? vehicleId = null,
            [FromQuery] int? airportId = null,
            [FromQuery] bool? isFromAirport = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var filters = new TransferFilterParameters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status,
                    GuestId = guestId,
                    PersonnelId = personnelId,
                    VehicleId = vehicleId,
                    AirportId = airportId,
                    IsFromAirport = isFromAirport,
                    SearchTerm = searchTerm
                };

                var result = await _exportService.ExportTransfersToExcelAsync(filters);
                
                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dışa aktarma başarısız oldu.", 500);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Dışa aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Transfer listesini CSV formatında dışa aktarır
        /// </summary>
        [HttpGet("transfers/csv")]
        public async Task<IActionResult> ExportTransfersToCsv(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null,
            [FromQuery] int? guestId = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] int? vehicleId = null,
            [FromQuery] int? airportId = null,
            [FromQuery] bool? isFromAirport = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var filters = new TransferFilterParameters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status,
                    GuestId = guestId,
                    PersonnelId = personnelId,
                    VehicleId = vehicleId,
                    AirportId = airportId,
                    IsFromAirport = isFromAirport,
                    SearchTerm = searchTerm
                };

                var result = await _exportService.ExportTransfersToCsvAsync(filters);
                
                if (!result.IsSuccess)
                    return Error(result.ErrorMessage ?? "Dışa aktarma başarısız oldu.", 500);

                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return Error($"Dışa aktarma sırasında hata oluştu: {ex.Message}", 500);
            }
        }
    }
}

