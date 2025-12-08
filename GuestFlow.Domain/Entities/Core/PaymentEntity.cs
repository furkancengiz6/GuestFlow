using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Ödeme entity'si
    /// </summary>
    public class PaymentEntity : BaseEntity
    {
        /// <summary>
        /// Ödeme numarası (benzersiz)
        /// </summary>
        public string PaymentNumber { get; set; }

        /// <summary>
        /// Fatura ID (ödemesi yapılan fatura)
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Misafir ID
        /// </summary>
        public int GuestId { get; set; }

        /// <summary>
        /// Ödeme tutarı
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Para birimi
        /// </summary>
        public string Currency { get; set; } = "TRY";

        /// <summary>
        /// Ödeme yöntemi
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Ödeme durumu
        /// </summary>
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Ödeme tarihi
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Ödeme gateway transaction ID (işlem ID'si)
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Ödeme gateway response (JSON formatında)
        /// </summary>
        public string? GatewayResponse { get; set; }

        /// <summary>
        /// İade tarihi (eğer iade edildiyse)
        /// </summary>
        public DateTime? RefundDate { get; set; }

        /// <summary>
        /// İade nedeni
        /// </summary>
        public string? RefundReason { get; set; }

        /// <summary>
        /// İptal nedeni
        /// </summary>
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }

        // Relational Properties
        public virtual InvoicesEntity Invoice { get; set; }
        public virtual GuestEntity Guest { get; set; }
    }

    /// <summary>
    /// Payment entity yapılandırması
    /// </summary>
    public class PaymentConfiguration : BaseConfiguration<PaymentEntity>
    {
        public override void Configure(EntityTypeBuilder<PaymentEntity> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.PaymentNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(p => p.PaymentNumber)
                .IsUnique();

            builder.Property(p => p.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(p => p.PaymentMethod)
                .HasConversion(
                    v => PaymentMethodHelper.ToString(v),
                    v => PaymentMethodHelper.FromString(v))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion(
                    v => PaymentStatusHelper.ToString(v),
                    v => PaymentStatusHelper.FromString(v))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.TransactionId)
                .HasMaxLength(200);

            builder.Property(p => p.GatewayResponse)
                .HasMaxLength(4000);

            builder.Property(p => p.RefundReason)
                .HasMaxLength(500);

            builder.Property(p => p.CancellationReason)
                .HasMaxLength(500);

            builder.Property(p => p.Notes)
                .HasMaxLength(1000);

            // Foreign Key Relationships
            builder.HasOne(p => p.Invoice)
                .WithMany()
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Guest)
                .WithMany()
                .HasForeignKey(p => p.GuestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

