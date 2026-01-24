// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Communication.Dtos
{
    /// <summary>
    /// Unified Communication History DTO (tüm iletişim kanalları birleşik)
    /// </summary>
    public class UnifiedCommunicationHistoryDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public List<CommunicationItemDto> Communications { get; set; } = new List<CommunicationItemDto>();
        public CommunicationSummaryDto Summary { get; set; } = new CommunicationSummaryDto();
    }

    /// <summary>
    /// İletişim öğesi DTO (e-posta, SMS, WhatsApp, in-app)
    /// </summary>
    public class CommunicationItemDto
    {
        public int Id { get; set; }
        public string Channel { get; set; } = string.Empty; // Email, SMS, WhatsApp, InApp
        public string Direction { get; set; } = string.Empty; // Inbound, Outbound
        public string Subject { get; set; } = string.Empty; // E-posta konusu veya SMS/WhatsApp başlığı
        public string Content { get; set; } = string.Empty; // Mesaj içeriği
        public DateTime SentDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string Status { get; set; } = string.Empty; // Sent, Delivered, Failed, Read
        public string? ErrorMessage { get; set; }
        public string? TemplateName { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? Provider { get; set; } // SMS/WhatsApp provider
        public string? MessageId { get; set; } // External message ID
        public int? PersonnelId { get; set; } // Gönderen personel
        public string? PersonnelName { get; set; }
        public string Source { get; set; } = "GuestFlow"; // GuestFlow, PMS
    }

    /// <summary>
    /// İletişim özeti DTO
    /// </summary>
    public class CommunicationSummaryDto
    {
        public int TotalCommunications { get; set; }
        public int EmailCount { get; set; }
        public int SmsCount { get; set; }
        public int WhatsAppCount { get; set; }
        public int InAppCount { get; set; }
        public int InboundCount { get; set; }
        public int OutboundCount { get; set; }
        public DateTime? LastCommunicationDate { get; set; }
    }
}
