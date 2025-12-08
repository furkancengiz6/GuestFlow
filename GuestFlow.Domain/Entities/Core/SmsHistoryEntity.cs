using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// SMS gönderim geçmişi entity'si
    /// </summary>
    public class SmsHistoryEntity : BaseEntity
    {
        /// <summary>
        /// Alıcı telefon numarası
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// SMS içeriği
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// SMS durumu
        /// </summary>
        public SmsStatus Status { get; set; } = SmsStatus.Pending;

        /// <summary>
        /// Gönderim tarihi
        /// </summary>
        public DateTime SentDate { get; set; }

        /// <summary>
        /// Teslim tarihi (eğer teslim edildiyse)
        /// </summary>
        public DateTime? DeliveredDate { get; set; }

        /// <summary>
        /// Hata mesajı (varsa)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// SMS gateway provider (Netgsm, Twilio, vb.)
        /// </summary>
        public string? Provider { get; set; }

        /// <summary>
        /// Gateway'den dönen mesaj ID
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Gateway yanıtı (JSON formatında)
        /// </summary>
        public string? GatewayResponse { get; set; }

        /// <summary>
        /// Şablon adı (varsa)
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// İlişkili entity tipi (Transfer, CityTour, YachtTour, Reservation, vb.)
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// İlişkili entity ID
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Misafir ID (varsa)
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Personel ID (varsa)
        /// </summary>
        public int? PersonnelId { get; set; }

        /// <summary>
        /// SMS tipi (Reminder, Confirmation, Notification, vb.)
        /// </summary>
        public string? SmsType { get; set; }

        // Relational Properties
        public virtual GuestEntity? Guest { get; set; }
        public virtual PersonnelEntity? Personnel { get; set; }
    }

    /// <summary>
    /// SmsHistory entity yapılandırması
    /// </summary>
    public class SmsHistoryConfiguration : BaseConfiguration<SmsHistoryEntity>
    {
        public override void Configure(EntityTypeBuilder<SmsHistoryEntity> builder)
        {
            base.Configure(builder);

            builder.Property(s => s.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(s => s.Message)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(s => s.Status)
                .HasConversion(
                    v => SmsStatusHelper.ToString(v),
                    v => SmsStatusHelper.FromString(v))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(s => s.Provider)
                .HasMaxLength(50);

            builder.Property(s => s.MessageId)
                .HasMaxLength(200);

            builder.Property(s => s.GatewayResponse)
                .HasMaxLength(4000);

            builder.Property(s => s.TemplateName)
                .HasMaxLength(100);

            builder.Property(s => s.RelatedEntityType)
                .HasMaxLength(50);

            builder.Property(s => s.SmsType)
                .HasMaxLength(50);

            builder.Property(s => s.ErrorMessage)
                .HasMaxLength(500);

            // Foreign Key Relationships
            builder.HasOne(s => s.Guest)
                .WithMany()
                .HasForeignKey(s => s.GuestId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(s => s.Personnel)
                .WithMany()
                .HasForeignKey(s => s.PersonnelId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

