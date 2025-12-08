using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.SmsModels
{
    /// <summary>
    /// SMS gönderme request modeli
    /// </summary>
    public class SendSmsRequest
    {
        [Required(ErrorMessage = "Telefon numarası gereklidir.")]
        [RegularExpression(@"^(\+90|0)?[5][0-9]{9}$", ErrorMessage = "Geçersiz telefon numarası formatı.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "SMS mesajı gereklidir.")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "SMS mesajı 1-1000 karakter arasında olmalıdır.")]
        public string Message { get; set; }

        public int? GuestId { get; set; }

        public int? PersonnelId { get; set; }

        public string? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }

        public string? SmsType { get; set; }

        public string? TemplateName { get; set; }
    }
}

