// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Application.Models.Requests.PMS
{
    public class CreatePMSIntegrationRequest
    {
        [Required]
        [StringLength(100)]
        public string ProviderName { get; set; } = string.Empty; // Opera, Elektraweb

        [Required]
        [StringLength(50)]
        public string ProviderCode { get; set; } = string.Empty; // OPERA, ELEKTRAWEB

        [Required]
        [Url]
        public string ApiEndpoint { get; set; } = string.Empty;

        [Required]
        public string ApiKey { get; set; } = string.Empty;

        public string? ApiSecret { get; set; }

        [Url]
        public string? WebhookUrl { get; set; }

        public string? WebhookSecret { get; set; }

        public bool IsActive { get; set; } = true;

        public string SyncMode { get; set; } = "Polling"; // RealTime, Polling, Batch

        [Range(1, 60)]
        public int PollingIntervalMinutes { get; set; } = 5;
    }

    public class UpdatePMSIntegrationRequest
    {
        [StringLength(100)]
        public string? ProviderName { get; set; }

        [Url]
        public string? ApiEndpoint { get; set; }

        public string? ApiKey { get; set; }

        public string? ApiSecret { get; set; }

        [Url]
        public string? WebhookUrl { get; set; }

        public string? WebhookSecret { get; set; }

        public bool? IsActive { get; set; }

        public string? SyncMode { get; set; }

        [Range(1, 60)]
        public int? PollingIntervalMinutes { get; set; }
    }
}
