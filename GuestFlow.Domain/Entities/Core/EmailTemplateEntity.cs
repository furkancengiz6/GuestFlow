using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// E-posta şablonu entity'si
    /// </summary>
    public class EmailTemplateEntity : BaseEntity
    {
        /// <summary>
        /// Şablon adı (unique)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Şablon başlığı/açıklaması
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// E-posta konusu (şablon değişkenleri içerebilir: {{VariableName}})
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// E-posta içeriği (HTML, şablon değişkenleri içerebilir)
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Şablon kategorisi
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Şablon değişkenleri açıklaması (JSON formatında)
        /// </summary>
        public string? VariablesDescription { get; set; }

        /// <summary>
        /// Aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Varsayılan şablon mu? (silinemez)
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Son güncelleme tarihi
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Güncelleyen personel ID
        /// </summary>
        public int? ModifiedByPersonnelId { get; set; }
    }

    /// <summary>
    /// EmailTemplate entity yapılandırması
    /// </summary>
    public class EmailTemplateConfiguration : BaseConfiguration<EmailTemplateEntity>
    {
        public override void Configure(EntityTypeBuilder<EmailTemplateEntity> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Subject)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.Body)
                .IsRequired();

            builder.Property(e => e.Category)
                .HasMaxLength(100);

            builder.Property(e => e.VariablesDescription);

            // Unique index
            builder.HasIndex(e => e.Name)
                .IsUnique();

            // Index'ler
            builder.HasIndex(e => e.Category);
            builder.HasIndex(e => e.IsActive);
        }
    }
}

