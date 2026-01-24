using GuestFlow.Application.Models.Responses.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Auth
{
    /// <summary>
    /// Login audit service interface
    /// Provides access to login attempt history for security monitoring
    /// </summary>
    public interface ILoginAuditService
    {
        /// <summary>
        /// Get login attempts with filtering
        /// </summary>
        Task<List<LoginAttemptDto>> GetLoginAttemptsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? email = null,
            string? ipAddress = null,
            bool? isSuccessful = null,
            int? personnelId = null,
            int? pageNumber = null,
            int? pageSize = null);

        /// <summary>
        /// Get login attempt statistics
        /// </summary>
        Task<LoginAuditStatisticsDto> GetStatisticsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Get failed login attempts count by email
        /// </summary>
        Task<List<FailedLoginSummaryDto>> GetFailedLoginSummaryAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? topCount = null);
    }
}
