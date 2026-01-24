// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Intelligence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Persistence.Configurations
{
    public class StaffBehaviorConfiguration : IEntityTypeConfiguration<StaffBehaviorEntity>
    {
        public void Configure(EntityTypeBuilder<StaffBehaviorEntity> builder)
        {

            builder.Property(b => b.BehaviorType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(b => b.Category)
                .HasMaxLength(100);

            builder.Property(b => b.BehaviorValue)
                .HasMaxLength(4000);

            builder.Property(b => b.ServiceType)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(b => b.StaffId);
            builder.HasIndex(b => new { b.StaffId, b.BehaviorType });
            builder.HasIndex(b => new { b.StaffId, b.BehaviorDate });
            builder.HasIndex(b => new { b.StaffId, b.GuestId });
            builder.HasIndex(b => new { b.StaffId, b.SyncedToGraph });

            // Foreign keys
            builder.HasOne(b => b.Staff)
                .WithMany()
                .HasForeignKey(b => b.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Guest)
                .WithMany()
                .HasForeignKey(b => b.GuestId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
