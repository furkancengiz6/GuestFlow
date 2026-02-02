using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Finance.Pricing;
using GuestFlow.Domain.Entities.Finance;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/pricing")]
    [ApiVersion("1.0")]
    public class PricingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDynamicPricingService _pricingService;

        public PricingController(IUnitOfWork unitOfWork, IDynamicPricingService pricingService)
        {
            _unitOfWork = unitOfWork;
            _pricingService = pricingService;
        }

        [HttpGet("rules")]
        public async Task<ActionResult<List<PricingRuleEntity>>> GetRules()
        {
            var rules = await _unitOfWork.PricingRules.GetAll().ToListAsync();
            return Ok(rules);
        }

        [HttpPost("rules")]
        public async Task<ActionResult<PricingRuleEntity>> CreateRule(PricingRuleEntity rule)
        {
             // Basic validation (can be moved to FluentValidation)
             if (rule.Priority < 0)
                 return BadRequest("Priority must be >= 0");

            await _unitOfWork.PricingRules.AddAsync(rule);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRules), new { id = rule.Id }, rule);
        }
        
        [HttpPost("calculate-test")]
        public async Task<ActionResult<DynamicPricingResult>> CalculateTest([FromQuery] int roomTypeId, [FromQuery] decimal baseRate, [FromQuery] DateTime date)
        {
            var result = await _pricingService.CalculateRateAsync(roomTypeId, date, baseRate);
            return Ok(result);
        }
    }
}
