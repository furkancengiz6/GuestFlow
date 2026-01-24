// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Intelligence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Persistence.Configurations
{
    public class GuestBehaviorConfiguration : IEntityTypeConfiguration<GuestBehaviorEntity>
    {
        public void Configure(EntityTypeBuilder<GuestBehaviorEntity> builder)
        {

            builder.Property(b => b.BehaviorType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(b => b.Category)
                .HasMaxLength(100);

            builder.Property(b => b.BehaviorValue)
                .HasMaxLength(4000);

            builder.Property(b => b.TimeOfDay)
                .HasMaxLength(20);

            builder.Property(b => b.DayOfWeek)
                .HasMaxLength(20);

            builder.Property(b => b.Season)
                .HasMaxLength(20);

            builder.Property(b => b.Currency)
                .HasMaxLength(10);

            builder.Property(b => b.RelatedEntityType)
                .HasMaxLength(100);

            // BaseEntity properties are handled by default conventions
            // Id, CreatedAt, UpdatedAt, IsDeleted are auto-configured

            // Indexes
            builder.HasIndex(b => b.GuestId);
            builder.HasIndex(b => new { b.GuestId, b.BehaviorType });
            builder.HasIndex(b => new { b.GuestId, b.BehaviorDate });
            builder.HasIndex(b => new { b.GuestId, b.SyncedToGraph });

            // Foreign key
            builder.HasOne(b => b.Guest)
                .WithMany()
                .HasForeignKey(b => b.GuestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
