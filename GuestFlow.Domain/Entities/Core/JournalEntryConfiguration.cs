// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    public class JournalEntryConfiguration : BaseConfiguration<JournalEntry>
    {
        public override void Configure(EntityTypeBuilder<JournalEntry> builder)
        {
            base.Configure(builder);

            builder.Property(j => j.Currency)
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("USD");

            builder.Property(j => j.PostedBy)
                .HasMaxLength(200);

            builder.Property(j => j.CreatedBy)
                .HasMaxLength(200);

            builder.Property(j => j.Description)
                .HasMaxLength(500);

            builder.Property(j => j.TotalDebit)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(j => j.TotalCredit)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(j => j.ReversedBy)
                .HasMaxLength(200);

            // Foreign keys for Personnel (hybrid approach: ID + Snapshot)
            // ID for referential integrity and joins, Snapshot for historical accuracy
            builder.HasOne(j => j.CreatedByPersonnel)
                .WithMany()
                .HasForeignKey(j => j.CreatedByPersonnelId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from SetNull to avoid cycles

            builder.HasOne(j => j.PostedByPersonnel)
                .WithMany()
                .HasForeignKey(j => j.PostedByPersonnelId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from SetNull to avoid cycles

            builder.HasOne(j => j.ReversedByPersonnel)
                .WithMany()
                .HasForeignKey(j => j.ReversedByPersonnelId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from SetNull to avoid cycles

            // Indexes
            builder.HasIndex(j => j.PostingDate);
            builder.HasIndex(j => j.ReversedByJournalEntryId)
                .HasFilter("[ReversedByJournalEntryId] IS NOT NULL");
            builder.HasIndex(j => j.CreatedByPersonnelId);
            builder.HasIndex(j => j.PostedByPersonnelId);
            builder.HasIndex(j => j.ReversedByPersonnelId);
        }
    }
}
