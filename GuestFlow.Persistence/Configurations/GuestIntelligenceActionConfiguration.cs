// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Intelligence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Persistence.Configurations
{
    public class GuestIntelligenceActionConfiguration : IEntityTypeConfiguration<GuestIntelligenceActionEntity>
    {
        public void Configure(EntityTypeBuilder<GuestIntelligenceActionEntity> builder)
        {
            builder.ToTable("GuestIntelligenceActions");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ActionType).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
            builder.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
            builder.Property(e => e.ExecutionDetails).HasMaxLength(2000);

            builder.HasOne(e => e.Guest)
                .WithMany()
                .HasForeignKey(e => e.GuestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.GuestId);
            builder.HasIndex(e => e.ExecutionDate);
            builder.HasIndex(e => e.Status);
        }
    }
}
