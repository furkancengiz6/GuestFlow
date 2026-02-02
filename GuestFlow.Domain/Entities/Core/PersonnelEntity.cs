using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class PersonnelEntity : BaseEntity, IPersonnel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        // Two-Factor Authentication (2FA) - Required for Admin/Owner
        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecret { get; set; } // Base32 encoded secret for TOTP
        public string? TwoFactorRecoveryCodes { get; set; } // JSON array of recovery codes (encrypted)
        public DateTime? TwoFactorSetupDate { get; set; }

        // Relational Properties
        public virtual ICollection<TransferEntity> Transfers { get; set; } = new List<TransferEntity>();
        public virtual ICollection<YachtTourEntity> YachtTours { get; set; } = new List<YachtTourEntity>();
        public virtual ICollection<CityTourEntity> CityTours { get; set; } = new List<CityTourEntity>();
        public virtual ICollection<DailyNoteEntity> DailyNotes { get; set; } = new List<DailyNoteEntity>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
    }

    public class PersonnelConfiguration : BaseConfiguration<PersonnelEntity>
    {
        public override void Configure(EntityTypeBuilder<PersonnelEntity> builder)
        {
            base.Configure(builder);
            builder.Property(p => p.FullName).HasMaxLength(200);
            builder.Property(p => p.Email).HasMaxLength(255);
            builder.Property(p => p.Password).HasMaxLength(256); // Şifrelenmiş hali için 
            builder.Property(p => p.TwoFactorSecret).HasMaxLength(100);
            builder.Property(p => p.TwoFactorRecoveryCodes).HasMaxLength(2000); // Encrypted recovery codes
            builder.HasIndex(p => p.Email).IsUnique();
        }
    }
}
