// GuestFlow.Domain/Entities/Core/PersonnelEntity.cs
using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class PersonnelEntity : BaseEntity, IPersonnel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } 
        public UserType UserType { get; set; }

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
            builder.HasIndex(x => x.Email)
               .IsUnique();
        }
    }
}