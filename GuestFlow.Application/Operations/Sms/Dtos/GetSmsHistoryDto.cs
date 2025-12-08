namespace GuestFlow.Application.Operations.Sms.Dtos
{
    /// <summary>
    /// SMS geçmişi DTO'su
    /// </summary>
    public class GetSmsHistoryDto
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime SentDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Provider { get; set; }
        public string? MessageId { get; set; }
        public string? TemplateName { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public int? GuestId { get; set; }
        public string? GuestName { get; set; }
        public int? PersonnelId { get; set; }
        public string? PersonnelName { get; set; }
        public string? SmsType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

