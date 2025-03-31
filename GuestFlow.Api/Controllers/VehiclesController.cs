
using GuestFlow.Api.Models.VehicleModels;
using GuestFlow.Application.Operations.Vehicle;
using GuestFlow.Application.Operations.Vehicle.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle(AddVehicleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addVehicleDto = new AddVehicleDto
            {
                Type = request.Type,
                PlateNumber = request.PlateNumber,
                Capacity = request.Capacity,
                DailyPrice = request.DailyPrice
            };

            var result = await _vehicleService.AddVehicle(addVehicleDto);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicles()
        {
            var result = await _vehicleService.GetVehicles();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _vehicleService.GetVehicleById(id);
            if (result == null)
            {
                return NotFound("Araç bulunamadı.");
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateVehicleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateVehicleDto = new UpdateVehicleDto
            {
                Id = id,
                Type = request.Type,
                PlateNumber = request.PlateNumber,
                Capacity = request.Capacity,
                DailyPrice = request.DailyPrice
            };

            var result = await _vehicleService.UpdateVehicle(updateVehicleDto);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _vehicleService.DeleteVehicle(id);
            if (result.IsSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
    }
}