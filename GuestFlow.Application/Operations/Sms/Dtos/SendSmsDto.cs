namespace GuestFlow.Application.Operations.Sms.Dtos
{
    /// <summary>
    /// SMS gönderme DTO'su
    /// </summary>
    public class SendSmsDto
    {
        /// <summary>
        /// Alıcı telefon numarası
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// SMS içeriği
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Misafir ID (varsa)
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Personel ID (varsa)
        /// </summary>
        public int? PersonnelId { get; set; }

        /// <summary>
        /// İlişkili entity tipi (Transfer, CityTour, YachtTour, Reservation, vb.)
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// İlişkili entity ID
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// SMS tipi (Reminder, Confirmation, Notification, vb.)
        /// </summary>
        public string? SmsType { get; set; }

        /// <summary>
        /// Şablon adı (varsa)
        /// </summary>
        public string? TemplateName { get; set; }
    }
}

