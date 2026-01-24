using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Models.Responses.Auth
{
    /// <summary>
    /// Login attempt DTO
    /// </summary>
    public class LoginAttemptDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
        public DateTime AttemptDate { get; set; }
        public int? PersonnelId { get; set; }
        public string? PersonnelName { get; set; }
    }

    /// <summary>
    /// Login audit statistics DTO
    /// </summary>
    public class LoginAuditStatisticsDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalAttempts { get; set; }
        public int SuccessfulAttempts { get; set; }
        public int FailedAttempts { get; set; }
        public decimal SuccessRate { get; set; }
        public int UniqueUsers { get; set; }
        public int UniqueIpAddresses { get; set; }
        public Dictionary<string, int> FailureReasons { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AttemptsByHour { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// Failed login summary DTO
    /// </summary>
    public class FailedLoginSummaryDto
    {
        public string Email { get; set; } = string.Empty;
        public int? PersonnelId { get; set; }
        public string? PersonnelName { get; set; }
        public int FailedAttemptCount { get; set; }
        public DateTime LastFailedAttempt { get; set; }
        public string? LastIpAddress { get; set; }
        public string? MostCommonFailureReason { get; set; }
    }
}
