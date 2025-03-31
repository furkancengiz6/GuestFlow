using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class DailyNote : BaseEntity, IDailyNote
    {
        public DateTime NoteDate { get; set; }
        public int RoomNumber { get; set; }
        public string NoteText { get; set; }
        public int? PersonnelId { get; set; }

        // Relational Property
        public virtual PersonnelEntity Personnel { get; set; }
    }

    public class DailyNoteConfiguration : BaseConfiguration<DailyNote>
    {
        public override void Configure(EntityTypeBuilder<DailyNote> builder)
        {
            base.Configure(builder);
          
        }
    }
}