using System;

namespace GuestFlow.Application.Operations.Review;

public class GuestReviewDto
{
    public int Id { get; set; }
    public int GuestId { get; set; }
    public string GuestFullName { get; set; }
    public int? ReservationId { get; set; }
    public int? ServiceId { get; set; }
    public string ServiceType { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public int? CleanlinessRating { get; set; }
    public int? ServiceQualityRating { get; set; }
    public int? StaffRating { get; set; }
    public bool IsApproved { get; set; }
    public string StaffResponse { get; set; }
    public DateTime? ResponseDate { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateGuestReviewDto
{
    public int GuestId { get; set; }
    public int? ReservationId { get; set; }
    public int? ServiceId { get; set; }
    public string ServiceType { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public int? CleanlinessRating { get; set; }
    public int? ServiceQualityRating { get; set; }
    public int? StaffRating { get; set; }
}

public class RespondToReviewDto
{
    public string Response { get; set; }
}
