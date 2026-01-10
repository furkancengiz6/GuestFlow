using GuestFlow.Application.Operations.Supplier;
using GuestFlow.Application;
using GuestFlow.Application.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    using Microsoft.AspNetCore.Authorization;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SupplierCostsController : ControllerBase
    {
        private readonly ISupplierCostService _supplierCostService;

        public SupplierCostsController(ISupplierCostService supplierCostService)
        {
            _supplierCostService = supplierCostService;
        }

        [HttpPost("sync")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> SyncSupplierCosts()
        {
            var result = await _supplierCostService.SyncSupplierCostsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _supplierCostService.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _supplierCostService.GetByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), 201)]
        public async Task<IActionResult> Create([FromBody] GuestFlow.Application.Models.Requests.Supplier.CreateSupplierCostRequest request)
        {
            var result = await _supplierCostService.CreateAsync(request);
            if (!result.Success)
                return BadRequest(result);

            if (result.Data is null)
                return StatusCode(StatusCodes.Status500InternalServerError, result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> Update(int id, [FromBody] GuestFlow.Application.Models.Requests.Supplier.UpdateSupplierCostRequest request)
        {
            var result = await _supplierCostService.UpdateAsync(id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _supplierCostService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Additional endpoints (CRUD) will be added as implementation matures
    }
}

