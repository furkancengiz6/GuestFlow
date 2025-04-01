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
        public string? Notes { get; set; }
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
            builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        }
    }
}
