using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Review;



public interface IReviewService
{
    Task<GuestFlow.Application.ApiResponse<GuestReviewDto>> CreateReviewAsync(CreateGuestReviewDto dto);
    Task<GuestFlow.Application.ApiResponse<List<GuestReviewDto>>> GetGuestReviewsAsync(int guestId);
    Task<GuestFlow.Application.ApiResponse<List<GuestReviewDto>>> GetPendingReviewsAsync();
    Task<GuestFlow.Application.ApiResponse<bool>> ApproveReviewAsync(int reviewId);
    Task<GuestFlow.Application.ApiResponse<bool>> RespondToReviewAsync(int reviewId, RespondToReviewDto dto);
    Task<GuestFlow.Application.ApiResponse<decimal>> GetAverageRatingAsync(string serviceType = null);
}
