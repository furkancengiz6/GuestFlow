using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Operations
{
    public class GuestYachtTour : BaseEntity
    {
        public int GuestId { get; set; }
        public int YachtTourId { get; set; }

        public virtual GuestEntity Guest { get; set; } = null!;
        public virtual YachtTourEntity YachtTour { get; set; } = null!;
    }

    public class GuestYachtTourConfiguration : BaseConfiguration<GuestYachtTour>
    {
        public override void Configure(EntityTypeBuilder<GuestYachtTour> builder)
        {
            // Composite key tanımı - BaseEntity Id'yi kullanmıyoruz
            builder.HasKey(gyt => new { gyt.GuestId, gyt.YachtTourId });

            // BaseEntity'den gelen property'leri override et (ignore değil)
            builder.Property(gyt => gyt.Id).ValueGeneratedNever(); // Id'yi kullanmıyoruz

            // BaseEntity'nin CreatedDate ve IsDeleted property'lerini ignore et
            // çünkü bu junction table'da bunları kullanmıyoruz
            builder.Ignore(gyt => gyt.CreatedDate);
            builder.Ignore(gyt => gyt.UpdatedDate);
            builder.Ignore(gyt => gyt.CreatedByPersonnelId);
            builder.Ignore(gyt => gyt.UpdatedByPersonnelId);
            builder.Ignore(gyt => gyt.IsDeleted);

            // Foreign key ilişkileri
            builder.HasOne(gyt => gyt.Guest)
                   .WithMany(g => g.GuestYachtTours)
                   .HasForeignKey(gyt => gyt.GuestId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(gyt => gyt.YachtTour)
                   .WithMany(yt => yt.GuestYachtTours)
                   .HasForeignKey(gyt => gyt.YachtTourId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Index'ler
            builder.HasIndex(gyt => gyt.GuestId);
            builder.HasIndex(gyt => gyt.YachtTourId);
            builder.HasIndex(gyt => new { gyt.GuestId, gyt.YachtTourId });
        }
    }
}
