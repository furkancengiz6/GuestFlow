using GuestFlow.Domain.Entities.Enum;
using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.ServicePackage.Dtos
{
    public class UpdateServicePackageDto
    {
        public int Id { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PackageType PackageType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string Currency { get; set; } = "TRY";
        public bool IsActive { get; set; } = true;
        public string? PackageContent { get; set; }
        public string? Notes { get; set; }
    }
}

