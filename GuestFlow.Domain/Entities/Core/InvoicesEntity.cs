using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Invoice status enum
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>Draft - can be edited</summary>
        Draft = 0,
        /// <summary>PDF Generated - LOCKED and immutable</summary>
        Generated = 1,
        /// <summary>Cancelled - cannot be used</summary>
        Cancelled = 2
    }
    
    /// <summary>
    /// Invoice entity with immutability support.
    /// 
    /// INVOICE REALITY (LOCKED PRODUCT DECISION):
    /// - Invoices are NOT created automatically at service creation
    /// - Invoice creation is time-based (checkout, end-of-day, manual)
    /// - One invoice may cover multiple services
    /// - Invoices are independent from payments
    /// - PDF generation LOCKS the invoice (immutable)
    /// - GuestFlow is NOT a tax authority
    /// </summary>
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
        public int? LockedByPersonnelId { get; set; }
        
        /// <summary>
        /// Invoice status (Draft, Generated, Cancelled)
        /// Once Generated (PDF created), invoice is LOCKED
        /// </summary>
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        
        /// <summary>
        /// True when PDF has been generated - invoice becomes IMMUTABLE
        /// </summary>
        public bool IsPdfGenerated { get; set; } = false;
        
        /// <summary>
        /// When the PDF was generated (point of immutability)
        /// </summary>
        public DateTime? PdfGeneratedDate { get; set; }
        
        /// <summary>
        /// Check if invoice can be modified (only Draft invoices, not PDF generated)
        /// INVOICE REALITY: Once PDF is generated, invoice becomes IMMUTABLE
        /// </summary>
        public bool CanBeModified => Status == InvoiceStatus.Draft && !IsPdfGenerated;
        
        /// <summary>
        /// Lock the invoice after PDF generation
        /// </summary>
        public void LockAfterPdfGeneration(string pdfUrl, int? lockedByPersonnelId = null)
        {
            IsPdfGenerated = true;
            PdfGeneratedDate = DateTime.UtcNow;
            PdfUrl = pdfUrl;
            Status = InvoiceStatus.Generated;
            LockedByPersonnelId = lockedByPersonnelId;
        }

        // Relational Properties
        public virtual GuestEntity Guest { get; set; }
        public virtual PersonnelEntity Personnel { get; set; }
        public virtual PersonnelEntity? LockedByPersonnel { get; set; }
        public virtual ICollection<InvoiceItemEntity> InvoiceItems { get; set; } = new List<InvoiceItemEntity>();
    }

    public class InvoicesConfiguration : BaseConfiguration<InvoicesEntity>
    {
        public override void Configure(EntityTypeBuilder<InvoicesEntity> builder)
        {
            base.Configure(builder);

            builder.Property(i => i.Currency).HasMaxLength(3); // USD, EUR, TRY gibi 3 karakterli para birimi kodları için
            builder.Property(i => i.Notes).HasMaxLength(1000);
            builder.Property(i => i.PdfUrl).HasMaxLength(500);
            builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
            builder.HasIndex(i => i.InvoiceNumber).IsUnique();
            
            // Invoice immutability fields - defaults are set in entity definition
            builder.Property(i => i.PdfGeneratedDate).IsRequired(false);

            // Add LockedByPersonnelId configuration
            builder.HasOne(i => i.LockedByPersonnel)
                .WithMany()
                .HasForeignKey(i => i.LockedByPersonnelId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
