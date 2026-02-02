using System;
using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Domain.Entities.Operations;

public class GuestReview : BaseEntity
{
    public int GuestId { get; set; }
    public GuestEntity Guest { get; set; }

    public int? ReservationId { get; set; }
    public ReservationEntity Reservation { get; set; }

    public int? ServiceId { get; set; } // General reference to a service if applicable
    public string ServiceType { get; set; } // Transfer, Tour, Restaurant etc.

    public int Rating { get; set; } // 1 to 5
    public string Comment { get; set; }
    
    // Category-specific ratings
    public int? CleanlinessRating { get; set; }
    public int? ServiceQualityRating { get; set; }
    public int? StaffRating { get; set; }
    
    public bool IsApproved { get; set; } = false;
    public string StaffResponse { get; set; }
    public DateTime? ResponseDate { get; set; }

}
