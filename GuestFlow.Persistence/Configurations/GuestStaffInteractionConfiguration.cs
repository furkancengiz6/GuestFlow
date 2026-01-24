// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Intelligence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Persistence.Configurations
{
    public class GuestStaffInteractionConfiguration : IEntityTypeConfiguration<GuestStaffInteractionEntity>
    {
        public void Configure(EntityTypeBuilder<GuestStaffInteractionEntity> builder)
        {

            builder.Property(i => i.InteractionType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(i => i.Channel)
                .HasMaxLength(50);

            builder.Property(i => i.Context)
                .HasMaxLength(4000);

            builder.Property(i => i.ServiceType)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(i => i.GuestId);
            builder.HasIndex(i => i.StaffId);
            builder.HasIndex(i => new { i.GuestId, i.StaffId });
            builder.HasIndex(i => new { i.GuestId, i.InteractionDate });
            builder.HasIndex(i => new { i.GuestId, i.StaffId, i.InteractionDate });
            builder.HasIndex(i => new { i.GuestId, i.SyncedToGraph });

            // Foreign keys
            builder.HasOne(i => i.Guest)
                .WithMany()
                .HasForeignKey(i => i.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Staff)
                .WithMany()
                .HasForeignKey(i => i.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
