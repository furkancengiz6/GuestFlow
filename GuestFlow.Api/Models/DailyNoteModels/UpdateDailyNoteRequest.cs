using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.DailyNoteModels
{
    public class UpdateDailyNoteRequest
    {
        [Required]
        public DateTime NoteDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int RoomNumber { get; set; }

        [Required]
        [StringLength(500)]
        public string NoteText { get; set; }

        public int? PersonnelId { get; set; }
    }
}