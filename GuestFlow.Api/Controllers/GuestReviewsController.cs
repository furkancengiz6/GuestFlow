using GuestFlow.Application.Operations.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1.0/[controller]")]
public class GuestReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public GuestReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateGuestReviewDto dto)
    {
        var result = await _reviewService.CreateReviewAsync(dto);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("guest/{guestId}")]
    public async Task<IActionResult> GetGuestReviews(int guestId)
    {
        var result = await _reviewService.GetGuestReviewsAsync(guestId);
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetPendingReviews()
    {
        var result = await _reviewService.GetPendingReviewsAsync();
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> ApproveReview(int id)
    {
        var result = await _reviewService.ApproveReviewAsync(id);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("{id}/respond")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> RespondToReview(int id, [FromBody] RespondToReviewDto dto)
    {
        var result = await _reviewService.RespondToReviewAsync(id, dto);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("average")]
    public async Task<IActionResult> GetAverageRating([FromQuery] string serviceType)
    {
        var result = await _reviewService.GetAverageRatingAsync(serviceType);
        return Ok(result);
    }
}
