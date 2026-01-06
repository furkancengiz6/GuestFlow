using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Invoice item representing a service included in an invoice.
    /// INVOICE REALITY (LOCKED PRODUCT DECISION):
    /// - One invoice may include multiple services
    /// - Each item stores service snapshot at invoicing time
    /// - Items cannot change after invoice PDF generation (lock)
    /// </summary>
    public class InvoiceItemEntity : BaseEntity
    {
        public int InvoiceId { get; set; }
        public string ServiceType { get; set; } = string.Empty; // "Transfer", "CityTour", "YachtTour"
        public int ServiceId { get; set; }
        public decimal Amount { get; set; } // Service price snapshot at invoicing time
        public string Currency { get; set; } = "TRY";
        public string? Notes { get; set; }

        // Relational Properties
        public virtual InvoicesEntity Invoice { get; set; } = null!;
    }

    public class InvoiceItemConfiguration : BaseConfiguration<InvoiceItemEntity>
    {
        public override void Configure(EntityTypeBuilder<InvoiceItemEntity> builder)
        {
            base.Configure(builder);

            builder.Property(ii => ii.ServiceType)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(ii => ii.ServiceId)
                .IsRequired();

            builder.Property(ii => ii.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(ii => ii.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(ii => ii.Notes)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.HasOne(ii => ii.Invoice)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade); // If invoice deleted, delete items

            // Ensure no duplicate service references in same invoice
            builder.HasIndex(ii => new { ii.InvoiceId, ii.ServiceType, ii.ServiceId })
                .IsUnique();
        }
    }
}
