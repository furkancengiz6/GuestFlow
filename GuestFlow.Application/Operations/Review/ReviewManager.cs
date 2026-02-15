using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Domain.Events;

namespace GuestFlow.Application.Operations.Review;



public class ReviewManager : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<GuestReview> _reviewRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewManager> _logger;
    private readonly INotificationService _notificationService;

    public ReviewManager(
        IUnitOfWork unitOfWork,
        IRepository<GuestReview> reviewRepository,
        IMapper mapper,
        ILogger<ReviewManager> logger,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _reviewRepository = reviewRepository;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<GuestFlow.Application.ApiResponse<GuestReviewDto>> CreateReviewAsync(CreateGuestReviewDto dto)
    {
        try
        {
            var review = new GuestReview
            {
                GuestId = dto.GuestId,
                ReservationId = dto.ReservationId,
                ServiceId = dto.ServiceId,
                ServiceType = dto.ServiceType,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CleanlinessRating = dto.CleanlinessRating,
                ServiceQualityRating = dto.ServiceQualityRating,
                StaffRating = dto.StaffRating,
                IsApproved = false // Requires approval by default or as per policy
            };

            review.AddDomainEvent(new GuestReviewAddedEvent(review));

            await _reviewRepository.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Notify staff about new review
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(dto.GuestId);
                await _notificationService.CreateAndSendNotificationAsync(new GuestFlow.Application.Operations.Notification.Dtos.CreateNotificationDto
                {
                    Title = "Yeni Misafir Değerlendirmesi",
                    Content = $"{guest?.FullName ?? "Bir misafir"} yeni bir değerlendirme bıraktı. Puan: {dto.Rating}/5",
                    NotificationType = "Push",
                    RelatedEntityType = "GuestReview",
                    RelatedEntityId = review.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for new review");
            }

            return GuestFlow.Application.ApiResponse<GuestReviewDto>.SuccessResponse(_mapper.Map<GuestReviewDto>(review));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return GuestFlow.Application.ApiResponse<GuestReviewDto>.Fail("Geri bildirim kaydedilirken bir hata oluştu.");
        }
    }

    public async Task<GuestFlow.Application.ApiResponse<List<GuestReviewDto>>> GetGuestReviewsAsync(int guestId)
    {
        var reviews = await _reviewRepository.GetAll(r => r.GuestId == guestId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
        
        return GuestFlow.Application.ApiResponse<List<GuestReviewDto>>.SuccessResponse(_mapper.Map<List<GuestReviewDto>>(reviews));
    }

    public async Task<GuestFlow.Application.ApiResponse<List<GuestReviewDto>>> GetPendingReviewsAsync()
    {
        var reviews = await _reviewRepository.GetAll(r => !r.IsApproved && !r.IsDeleted)
            .Include(r => r.Guest)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

        return GuestFlow.Application.ApiResponse<List<GuestReviewDto>>.SuccessResponse(_mapper.Map<List<GuestReviewDto>>(reviews));
    }

    public async Task<GuestFlow.Application.ApiResponse<bool>> ApproveReviewAsync(int reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return GuestFlow.Application.ApiResponse<bool>.Fail("Değerlendirme bulunamadı.");

        review.IsApproved = true;
        await _reviewRepository.UpdateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        return GuestFlow.Application.ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<GuestFlow.Application.ApiResponse<bool>> RespondToReviewAsync(int reviewId, RespondToReviewDto dto)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return GuestFlow.Application.ApiResponse<bool>.Fail("Değerlendirme bulunamadı.");

        review.StaffResponse = dto.Response;
        review.ResponseDate = DateTime.UtcNow;
        review.IsApproved = true; // Automatically approve when responded

        await _reviewRepository.UpdateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        return GuestFlow.Application.ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<GuestFlow.Application.ApiResponse<decimal>> GetAverageRatingAsync(string serviceType = null)
    {
        var query = _reviewRepository.GetAll(r => !r.IsDeleted && r.IsApproved);
        
        if (!string.IsNullOrEmpty(serviceType))
        {
            query = query.Where(r => r.ServiceType == serviceType);
        }

        if (!await query.AnyAsync()) return GuestFlow.Application.ApiResponse<decimal>.SuccessResponse(0);

        var avg = await query.AverageAsync(r => (decimal)r.Rating);
        return GuestFlow.Application.ApiResponse<decimal>.SuccessResponse(Math.Round(avg, 1));
    }
}
