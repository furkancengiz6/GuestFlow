using GuestFlow.Api.Models.GuestModels;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class GuestsController : ControllerBase
    {
        private readonly IGuestService _guestService;

        public GuestsController(IGuestService guestService)
        {
            _guestService = guestService;
        }

        [HttpPost]
        public async Task<IActionResult> AddGuest(AddGuestRequest request)
        {
            var addGuestDto = new AddGuestDto
            {
                FullName = request.FullName,
                Email = request.Email,
                Nationality = request.Nationality,
                PhoneNumber = request.PhoneNumber,
                IsSpecialGuest = request.IsSpecialGuest
            };

            var result = await _guestService.AddGuest(addGuestDto); // resutl -> result
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
        public async Task<IActionResult> GetGuests()
        {
            var result = await _guestService.GetGuests();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuestById(int id)
        {
            var result = await _guestService.GetGuestById(id);
            if (result is null)
            {
                return NotFound("Misafir bulunamadı.");
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateGuestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updateGuestDto = new UpdateGuestDto
            {
                Id = id,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Nationality = request.Nationality,
                IsSpecialGuest = request.IsSpecialGuest
            };
            var result = await _guestService.UpdateGuest(updateGuestDto);
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
            var result = await _guestService.DeleteGuest(id);
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