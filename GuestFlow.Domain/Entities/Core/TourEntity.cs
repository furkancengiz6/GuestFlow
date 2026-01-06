using GuestFlow.Domain.Entities.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Admin tarafından yönetilen tur tanımları (Kapadokya Turu, Pamukkale Turu vb.)
    /// </summary>
    public class TourEntity : BaseEntity, ITour
    {
        public string Name { get; set; } = string.Empty; // Tur adı (örn: "Kapadokya Turu")
        public string? Description { get; set; } // Tur açıklaması
        public int CityId { get; set; } // Turun yapılacağı şehir
        public bool IsActive { get; set; } = true; // Tur aktif mi?

        // Relational Properties
        public virtual CityEntity City { get; set; } = null!;
        public virtual ICollection<CityTourEntity> CityTours { get; set; } = new List<CityTourEntity>();
    }

    public class TourConfiguration : BaseConfiguration<TourEntity>
    {
        public override void Configure(EntityTypeBuilder<TourEntity> builder)
        {
            base.Configure(builder);
            builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
            builder.Property(t => t.Description).HasMaxLength(1000).IsRequired(false);
            
            builder.HasOne(t => t.City)
                .WithMany(c => c.Tours)
                .HasForeignKey(t => t.CityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

