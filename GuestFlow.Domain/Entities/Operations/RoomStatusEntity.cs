// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Operations
{
    /// <summary>
    /// Room status entity for housekeeping management
    /// Tracks cleaning status, occupancy, and housekeeper assignments
    /// </summary>
    public class RoomStatusEntity : BaseEntity
    {
        public string RoomNumber { get; set; } = string.Empty;
        public RoomCleaningStatus CleaningStatus { get; set; }
        public RoomOccupancyStatus OccupancyStatus { get; set; }
        public DateTime LastCleaned { get; set; }
        public DateTime? NextInspection { get; set; }
        public int? AssignedHousekeeperId { get; set; }
        public string? Notes { get; set; }
        
        // Foreign Keys
        public int? HotelId { get; set; }
        
        // Navigation Properties
        public virtual PersonnelEntity? AssignedHousekeeper { get; set; }
        public virtual HotelEntity? Hotel { get; set; }
    }

    public enum RoomCleaningStatus
    {
        Clean = 0,
        Dirty = 1,
        InProgress = 2,
        Inspected = 3,
        OutOfOrder = 4
    }

    public enum RoomOccupancyStatus
    {
        Vacant = 0,
        Occupied = 1,
        Reserved = 2,
        CheckedOut = 3
    }

    public class RoomStatusConfiguration : IEntityTypeConfiguration<RoomStatusEntity>
    {
        public void Configure(EntityTypeBuilder<RoomStatusEntity> builder)
        {
            builder.ToTable("RoomStatuses");
            
            builder.Property(r => r.RoomNumber)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(r => r.Notes)
                .HasMaxLength(500);
            
            builder.HasIndex(r => new { r.HotelId, r.RoomNumber })
                .IsUnique();
            
            builder.HasOne(r => r.AssignedHousekeeper)
                .WithMany()
                .HasForeignKey(r => r.AssignedHousekeeperId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.HasOne(r => r.Hotel)
                .WithMany()
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
