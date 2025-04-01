using GuestFlow.Api.Models.CityToursModels;
using GuestFlow.Application.Operations.CityTour;
using GuestFlow.Application.Operations.CityTour.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class CityToursController : ControllerBase
    {
        private readonly ICityTourService _cityTourService;

        public CityToursController(ICityTourService cityTourService)
        {

            _cityTourService = cityTourService;
        }
        [HttpPost]
        public async Task<IActionResult> AddCityTour( AddCityTourRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new AddCityTourDto
            {
                TourDate = request.TourDate,
                Language = request.Language,
                DurationHours = request.DurationHours,
                Price = request.Price,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId,
                CityId = request.CityId,
                CreateInvoice = request.CreateInvoice,
                DiscountPercentage = request.DiscountPercentage,
                InvoiceDescription = request.InvoiceDescription
            };

            var result = await _cityTourService.AddCityTour(dto);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }
    

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _cityTourService.GetCityTourById(id);
            if (result is null)
            {
                return NotFound("Şehir turu bulunamadı.");
            }
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetCityTours()
        {
            var result = await _cityTourService.GetCityTours();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCityTourRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updateCityTourDto = new UpdateCityTourDto
            {
                Id = id,
                TourDate = request.TourDate,
                Language = request.Language,
                DurationHours = request.DurationHours,
                Price = request.Price,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId,
                CityId = request.CityId 
            };
            var result = await _cityTourService.UpdateCityTour(updateCityTourDto);
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
            var result = await _cityTourService.DeleteCityTour(id);
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
