using GuestFlow.Api.Models.AirportModels;
using GuestFlow.Application.Operations.Airport.Dtos;
using GuestFlow.Application.Operations.Airport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class AirportsController : ControllerBase
    {
        private readonly IAirportService _airportService;

        public AirportsController(IAirportService airportService)
        {
            _airportService = airportService;
        }

        [HttpPost]
        public async Task<IActionResult> AddAirport(AddAirportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addAirportDto = new AddAirportDto
            {
                Name = request.Name,
                Code = request.Code,
                CityId = request.CityId
            };

            var result = await _airportService.AddAirport(addAirportDto);
            if (result.IsSuccess)
            {
                return Ok(new { Message = result.Message });
            }
            else
            {
                return BadRequest(new { Message = result.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetAirports()
        {
            var result = await _airportService.GetAirports();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _airportService.GetAirportById(id);
            if (result == null)
            {
                return NotFound(new { Message = "Havalimanı bulunamadı." });
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Update(int id, UpdateAirportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateAirportDto = new UpdateAirportDto
            {
                Id = id,
                Name = request.Name,
                Code = request.Code,
                CityId = request.CityId
            };

            var result = await _airportService.UpdateAirport(updateAirportDto);
            if (result.IsSuccess)
            {
                return Ok(new { Message = result.Message });
            }
            else
            {
                return BadRequest(new { Message = result.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _airportService.DeleteAirport(id);
            if (result.IsSuccess)
            {
                return Ok(new { Message = result.Message });
            }
            else
            {
                return BadRequest(new { Message = result.Message });
            }
        }
    }
}

