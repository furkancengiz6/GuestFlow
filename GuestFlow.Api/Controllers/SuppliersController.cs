using GuestFlow.Application.Models.Requests.Supplier;
using GuestFlow.Application.Operations.Profitability;
using GuestFlow.Application.Operations.Supplier;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Application.Models;
using GuestFlow.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SuppliersController : BaseController
    {
        private readonly ISupplierService _supplierService;
        private readonly IProfitabilityService _profitabilityService;

        public SuppliersController(
            ISupplierService supplierService,
            IProfitabilityService profitabilityService)
        {
            _supplierService = supplierService;
            _profitabilityService = profitabilityService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Supplier>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] bool? isActive)
        {
            var result = await _supplierService.GetAllSuppliersAsync(type, isActive);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Supplier>), 200)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _supplierService.GetSupplierByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("by-type/{type}")]
        [ProducesResponseType(typeof(ApiResponse<List<Supplier>>), 200)]
        public async Task<IActionResult> GetByType(string type)
        {
            var result = await _supplierService.GetSuppliersByTypeAsync(type);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Supplier>), 201)]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
        {
            var result = await _supplierService.CreateSupplierAsync(request);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Supplier>), 200)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequest request)
        {
            var result = await _supplierService.UpdateSupplierAsync(id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _supplierService.DeleteSupplierAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("profitability/report")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetProfitabilityReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? supplierId)
        {
            var result = await _profitabilityService.GetProfitabilityReportAsync(startDate, endDate, supplierId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("profitability/top-suppliers")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetTopSuppliersByProfit(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int topCount = 10)
        {
            var result = await _profitabilityService.GetTopSuppliersByProfitAsync(startDate, endDate, topCount);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}