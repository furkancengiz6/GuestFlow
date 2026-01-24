// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.WhatsApp.Dtos
{
    /// <summary>
    /// WhatsApp mesaj gönderme DTO
    /// </summary>
    public class SendWhatsAppDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? GuestId { get; set; }
        public int? PersonnelId { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? TemplateName { get; set; }
        public Dictionary<string, string>? TemplateParameters { get; set; }
        public WhatsAppMessageType MessageType { get; set; } = WhatsAppMessageType.Text;
        public WhatsAppRichMessage? RichMessage { get; set; }
    }

    /// <summary>
    /// WhatsApp mesaj tipi
    /// </summary>
    public enum WhatsAppMessageType
    {
        Text = 1,
        Template = 2,
        Interactive = 3,
        Document = 4,
        Image = 5,
        Location = 6
    }

    /// <summary>
    /// WhatsApp rich message (butonlu, interaktif mesajlar)
    /// </summary>
    public class WhatsAppRichMessage
    {
        public string? HeaderText { get; set; }
        public string? BodyText { get; set; }
        public string? FooterText { get; set; }
        public List<WhatsAppButton>? Buttons { get; set; }
        public string? DocumentUrl { get; set; }
        public string? DocumentName { get; set; }
        public string? ImageUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LocationName { get; set; }
    }

    /// <summary>
    /// WhatsApp buton
    /// </summary>
    public class WhatsAppButton
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public WhatsAppButtonType Type { get; set; } = WhatsAppButtonType.Reply;
        public string? Url { get; set; }
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// WhatsApp buton tipi
    /// </summary>
    public enum WhatsAppButtonType
    {
        Reply = 1,      // Yanıt butonu
        Url = 2,        // URL butonu
        Call = 3        // Arama butonu
    }

    /// <summary>
    /// WhatsApp geçmişi DTO
    /// </summary>
    public class GetWhatsAppHistoryDto
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public int? GuestId { get; set; }
        public string? GuestName { get; set; }
        public int? PersonnelId { get; set; }
        public string? PersonnelName { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? MessageId { get; set; }
        public string? GatewayResponse { get; set; }
        public string? ErrorMessage { get; set; }
        public WhatsAppMessageType MessageType { get; set; }
    }

    /// <summary>
    /// WhatsApp istatistikleri DTO
    /// </summary>
    public class WhatsAppStatisticsDto
    {
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalRead { get; set; }
        public int TotalFailed { get; set; }
        public int TotalPending { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal DeliveryRate { get; set; }
        public decimal ReadRate { get; set; }
        public Dictionary<string, int> MessagesByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> MessagesByStatus { get; set; } = new Dictionary<string, int>();
    }
}
