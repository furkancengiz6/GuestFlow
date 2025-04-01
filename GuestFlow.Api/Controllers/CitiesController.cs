using GuestFlow.Api.Models.CityModels;
using GuestFlow.Application.Operations.City;
using GuestFlow.Application.Operations.City.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityService _cityService;

        public CitiesController(ICityService cityService)
        {
            _cityService = cityService;
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddCityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new AddCityDto
            {
                CityName = request.CityName,
                Country = request.Country
            };

            var result = await _cityService.AddCity(dto);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new UpdateCityDto
            {
                Id = id,
                CityName = request.CityName,
                Country = request.Country
            };

            var result = await _cityService.UpdateCity(dto);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _cityService.DeleteCity(id);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _cityService.GetCityById(id);
            return result == null ? NotFound("Şehir bulunamadı.") : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCities()
        {
            var result = await _cityService.GetCities();
            return Ok(result);
        }
    }
}