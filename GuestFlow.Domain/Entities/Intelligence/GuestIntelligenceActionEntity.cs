// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Core;
using System;

namespace GuestFlow.Domain.Entities.Intelligence
{
    /// <summary>
    /// Audit trail for automatic and manual intelligence actions.
    /// </summary>
    public class GuestIntelligenceActionEntity : BaseEntity
    {
        public int GuestId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAutomatic { get; set; }
        public string Status { get; set; } = "Pending"; // Success, Failed, Pending
        public string? ExecutionDetails { get; set; }
        public double Confidence { get; set; }
        public DateTime ExecutionDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual GuestEntity Guest { get; set; } = null!;
    }
}
