// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.NotificationRules.Dtos
{
    /// <summary>
    /// Notification Rule DTO
    /// </summary>
    public class NotificationRuleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public string Conditions { get; set; } = string.Empty;
        public string NotificationChannel { get; set; } = "Email";
        public string? TemplateName { get; set; }
        public string RecipientType { get; set; } = "Guest";
        public int? RecipientId { get; set; }
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 5;
        public int CheckIntervalMinutes { get; set; } = 60;
        public DateTime? LastCheckedAt { get; set; }
        public DateTime? LastTriggeredAt { get; set; }
        public int TriggerCount { get; set; } = 0;
        public string? Parameters { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// Create/Update Notification Rule DTO
    /// </summary>
    public class UpsertNotificationRuleDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public string Conditions { get; set; } = string.Empty;
        public string NotificationChannel { get; set; } = "Email";
        public string? TemplateName { get; set; }
        public string RecipientType { get; set; } = "Guest";
        public int? RecipientId { get; set; }
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 5;
        public int CheckIntervalMinutes { get; set; } = 60;
        public string? Parameters { get; set; }
    }

    /// <summary>
    /// Rule condition model (JSON deserialization için)
    /// </summary>
    public class RuleCondition
    {
        public string EntityType { get; set; } = string.Empty; // Invoice, Transfer, CityTour, vb.
        public string Field { get; set; } = string.Empty; // DueDate, TransferDate, vb.
        public string Operator { get; set; } = string.Empty; // >, <, ==, !=, >=, <=
        public object? Value { get; set; } // Karşılaştırma değeri
        public string? ConditionExpression { get; set; } // Karmaşık koşullar için (örn: "DaysOverdue > 3")
    }

    /// <summary>
    /// Rule execution result
    /// </summary>
    public class RuleExecutionResult
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public bool Triggered { get; set; }
        public int MatchedEntitiesCount { get; set; }
        public int NotificationsSent { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
