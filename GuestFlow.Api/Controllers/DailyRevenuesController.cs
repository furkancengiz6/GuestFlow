
using GuestFlow.Api.Models.DailyRevenueModels;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.DailyRevenue.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class DailyRevenuesController : ControllerBase
    {
        private readonly IDailyRevenueService _dailyRevenueService;

        public DailyRevenuesController(IDailyRevenueService dailyRevenueService)
        {
            _dailyRevenueService = dailyRevenueService;
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddDailyRevenueRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new AddDailyRevenueDto
            {
                Date = request.Date,
                TotalRevenue = request.TotalRevenue
            };

            var result = await _dailyRevenueService.AddDailyRevenue(dto);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDailyRevenueRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new UpdateDailyRevenueDto
            {
                Id = id,
                Date = request.Date,
                TotalRevenue = request.TotalRevenue
            };

            var result = await _dailyRevenueService.UpdateDailyRevenue(dto);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _dailyRevenueService.DeleteDailyRevenue(id);
            return result.IsSuccess ? Ok(new { result.Message }) : BadRequest(new { result.Message });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _dailyRevenueService.GetDailyRevenueById(id);
            return result == null ? NotFound("Günlük gelir bulunamadı.") : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyRevenues()
        {
            var result = await _dailyRevenueService.GetDailyRevenues();
            return Ok(result);
        }
    }
}