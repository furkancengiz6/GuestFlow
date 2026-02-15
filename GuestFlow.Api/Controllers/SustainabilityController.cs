using System.Threading.Tasks;
using GuestFlow.Application.Operations.Sustainability;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SustainabilityController : ControllerBase
    {
        private readonly ISustainabilityService _sustainabilityService;

        public SustainabilityController(ISustainabilityService sustainabilityService)
        {
            _sustainabilityService = sustainabilityService;
        }

        [HttpPost("record-action")]
        public async Task<IActionResult> RecordAction([FromBody] SustainabilityActionRequest request)
        {
            var newScore = await _sustainabilityService.RecordActionAsync(request.GuestId, request.ActionType, request.Notes);
            return Ok(new { success = true, totalScore = newScore });
        }

        [HttpGet("reward-recommendation/{guestId}")]
        public async Task<IActionResult> GetRewardRecommendation(int guestId)
        {
            var recommendation = await _sustainabilityService.GetAIRewardRecommendationAsync(guestId);
            return Ok(new { success = true, recommendation });
        }

        [HttpGet("score/{guestId}")]
        public async Task<IActionResult> GetScore(int guestId)
        {
            var score = await _sustainabilityService.GetGuestScoreAsync(guestId);
            return Ok(new { success = true, score });
        }
    }

    public class SustainabilityActionRequest
    {
        public int GuestId { get; set; }
        public SustainabilityActionType ActionType { get; set; }
        public string? Notes { get; set; }
    }
}
