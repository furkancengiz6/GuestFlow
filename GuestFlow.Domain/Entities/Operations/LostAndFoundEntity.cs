// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Operations
{
    /// <summary>
    /// Lost and found items tracking entity
    /// </summary>
    public class LostAndFoundEntity : BaseEntity
    {
        public string ItemDescription { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime FoundDate { get; set; }
        public bool IsReturned { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string? StorageLocation { get; set; }
        public string? ItemCategory { get; set; } // Electronics, Jewelry, Clothing, Documents, Other
        
        // Foreign Keys
        public int FoundByPersonnelId { get; set; }
        public int? GuestId { get; set; }
        public int? HotelId { get; set; }
        
        // Navigation Properties
        public virtual PersonnelEntity FoundByPersonnel { get; set; } = null!;
        public virtual GuestEntity? Guest { get; set; }
        public virtual HotelEntity? Hotel { get; set; }
    }

    public class LostAndFoundConfiguration : IEntityTypeConfiguration<LostAndFoundEntity>
    {
        public void Configure(EntityTypeBuilder<LostAndFoundEntity> builder)
        {
            builder.ToTable("LostAndFoundItems");
            
            builder.Property(l => l.ItemDescription)
                .HasMaxLength(500)
                .IsRequired();
            
            builder.Property(l => l.RoomNumber)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(l => l.StorageLocation)
                .HasMaxLength(100);
            
            builder.Property(l => l.ItemCategory)
                .HasMaxLength(50);
            
            builder.HasOne(l => l.FoundByPersonnel)
                .WithMany()
                .HasForeignKey(l => l.FoundByPersonnelId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(l => l.Guest)
                .WithMany()
                .HasForeignKey(l => l.GuestId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.HasOne(l => l.Hotel)
                .WithMany()
                .HasForeignKey(l => l.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasIndex(l => l.IsReturned);
            builder.HasIndex(l => l.FoundDate);
        }
    }
}
