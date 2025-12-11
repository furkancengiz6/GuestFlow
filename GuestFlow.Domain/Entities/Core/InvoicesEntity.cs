using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace GuestFlow.Domain.Entities.Core
{
    public class InvoicesEntity : BaseEntity, IInvoice
    {
        public int InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string PdfUrl { get; set; }
        public int GuestId { get; set; }
        public int? PersonnelId { get; set; }
        public int? TransferId { get; set; }
        public int? YachtTourId { get; set; }
        public int? CityTourId { get; set; }

        // Relational Properties
        public virtual GuestEntity Guest { get; set; }
        public virtual PersonnelEntity Personnel { get; set; }
        public virtual TransferEntity Transfer { get; set; }
        public virtual YachtTourEntity YachtTour { get; set; }
        public virtual CityTourEntity CityTour { get; set; }
    }

    public class InvoicesConfiguration : BaseConfiguration<InvoicesEntity>
    {
        public override void Configure(EntityTypeBuilder<InvoicesEntity> builder)
        {
            base.Configure(builder);

            base.Configure(builder);
            builder.Property(i => i.Currency).HasMaxLength(3); // USD, EUR, TRY gibi 3 karakterli para birimi kodları için
            builder.Property(i => i.Notes).HasMaxLength(1000);
            builder.Property(i => i.PdfUrl).HasMaxLength(500);
            builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
            builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        }
    }
}
