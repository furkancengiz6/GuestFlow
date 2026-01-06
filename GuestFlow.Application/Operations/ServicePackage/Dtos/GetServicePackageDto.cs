using GuestFlow.Domain.Entities.Enum;
using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.ServicePackage.Dtos
{
    public class GetServicePackageDto
    {
        public int Id { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PackageType PackageType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public string Currency { get; set; } = "TRY";
        public bool IsActive { get; set; }
        public string? PackageContent { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<int> TransferIds { get; set; } = new List<int>();
        public List<int> CityTourIds { get; set; } = new List<int>();
        public List<int> YachtTourIds { get; set; } = new List<int>();
        public List<int> RestaurantReservationIds { get; set; } = new List<int>();
    }
}

