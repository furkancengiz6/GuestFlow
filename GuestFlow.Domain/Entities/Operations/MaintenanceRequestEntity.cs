// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Operations
{
    /// <summary>
    /// Maintenance request entity for tracking room and facility issues
    /// </summary>
    public class MaintenanceRequestEntity : BaseEntity
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public MaintenancePriority Priority { get; set; }
        public MaintenanceStatus Status { get; set; }
        public DateTime ReportedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolutionNotes { get; set; }
        
        // Foreign Keys
        public int ReportedByPersonnelId { get; set; }
        public int? AssignedToPersonnelId { get; set; }
        public int? HotelId { get; set; }
        
        // Navigation Properties
        public virtual PersonnelEntity ReportedByPersonnel { get; set; } = null!;
        public virtual PersonnelEntity? AssignedToPersonnel { get; set; }
        public virtual HotelEntity? Hotel { get; set; }
    }

    public enum MaintenancePriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }

    public enum MaintenanceStatus
    {
        Pending = 0,
        InProgress = 1,
        Resolved = 2,
        Cancelled = 3
    }

    public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequestEntity>
    {
        public void Configure(EntityTypeBuilder<MaintenanceRequestEntity> builder)
        {
            builder.ToTable("MaintenanceRequests");
            
            builder.Property(m => m.RoomNumber)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(m => m.IssueDescription)
                .HasMaxLength(1000)
                .IsRequired();
            
            builder.Property(m => m.ResolutionNotes)
                .HasMaxLength(1000);
            
            builder.HasOne(m => m.ReportedByPersonnel)
                .WithMany()
                .HasForeignKey(m => m.ReportedByPersonnelId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(m => m.AssignedToPersonnel)
                .WithMany()
                .HasForeignKey(m => m.AssignedToPersonnelId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.HasOne(m => m.Hotel)
                .WithMany()
                .HasForeignKey(m => m.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasIndex(m => m.Status);
            builder.HasIndex(m => m.Priority);
        }
    }
}
