// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    public class JournalLineConfiguration : BaseConfiguration<JournalLine>
    {
        public override void Configure(EntityTypeBuilder<JournalLine> builder)
        {
            base.Configure(builder);

            builder.Property(j => j.AccountCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(j => j.Currency)
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("USD");

            builder.Property(j => j.ExchangeRate)
                .HasPrecision(18, 6); // High precision for exchange rates

            builder.Property(j => j.Debit)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(j => j.Credit)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(j => j.Description)
                .HasMaxLength(500);

            builder.HasOne(j => j.JournalEntry)
                .WithMany(j => j.Lines)
                .HasForeignKey(j => j.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
