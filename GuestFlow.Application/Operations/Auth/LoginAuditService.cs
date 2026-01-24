using GuestFlow.Application.Models.Responses.Auth;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Auth
{
    /// <summary>
    /// Login audit service implementation
    /// Provides access to login attempt history for security monitoring
    /// </summary>
    public class LoginAuditService : ILoginAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoginAuditService> _logger;

        public LoginAuditService(
            IUnitOfWork unitOfWork,
            ILogger<LoginAuditService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<LoginAttemptDto>> GetLoginAttemptsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? email = null,
            string? ipAddress = null,
            bool? isSuccessful = null,
            int? personnelId = null,
            int? pageNumber = null,
            int? pageSize = null)
        {
            try
            {
                var query = _unitOfWork.LoginAttempts.GetAll();

                // Apply filters
                if (startDate.HasValue)
                    query = query.Where(l => l.AttemptDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(l => l.AttemptDate <= endDate.Value.AddDays(1).AddTicks(-1));

                if (!string.IsNullOrEmpty(email))
                    query = query.Where(l => l.Email.ToLower().Contains(email.ToLower()));

                if (!string.IsNullOrEmpty(ipAddress))
                    query = query.Where(l => l.IpAddress == ipAddress);

                if (isSuccessful.HasValue)
                    query = query.Where(l => l.IsSuccessful == isSuccessful.Value);

                if (personnelId.HasValue)
                    query = query.Where(l => l.PersonnelId == personnelId.Value);

                // Order by date (newest first)
                query = query.OrderByDescending(l => l.AttemptDate);

                // Apply pagination
                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    var skip = (pageNumber.Value - 1) * pageSize.Value;
                    query = query.Skip(skip).Take(pageSize.Value);
                }

                var attempts = await query
                    .Include(l => l.Personnel)
                    .ToListAsync();

                return attempts.Select(l => new LoginAttemptDto
                {
                    Id = l.Id,
                    Email = l.Email,
                    IpAddress = l.IpAddress,
                    IsSuccessful = l.IsSuccessful,
                    FailureReason = l.FailureReason,
                    AttemptDate = l.AttemptDate,
                    PersonnelId = l.PersonnelId,
                    PersonnelName = l.Personnel?.FullName
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login attempts");
                throw;
            }
        }

        public async Task<LoginAuditStatisticsDto> GetStatisticsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _unitOfWork.LoginAttempts.GetAll();

                if (startDate.HasValue)
                    query = query.Where(l => l.AttemptDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(l => l.AttemptDate <= endDate.Value.AddDays(1).AddTicks(-1));

                var attempts = await query.ToListAsync();

                var statistics = new LoginAuditStatisticsDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalAttempts = attempts.Count,
                    SuccessfulAttempts = attempts.Count(a => a.IsSuccessful),
                    FailedAttempts = attempts.Count(a => !a.IsSuccessful),
                    UniqueUsers = attempts.Select(a => a.Email.ToLower()).Distinct().Count(),
                    UniqueIpAddresses = attempts.Where(a => !string.IsNullOrEmpty(a.IpAddress))
                        .Select(a => a.IpAddress!)
                        .Distinct()
                        .Count()
                };

                // Calculate success rate
                if (statistics.TotalAttempts > 0)
                {
                    statistics.SuccessRate = (decimal)statistics.SuccessfulAttempts / statistics.TotalAttempts * 100;
                }

                // Failure reasons breakdown
                statistics.FailureReasons = attempts
                    .Where(a => !a.IsSuccessful && !string.IsNullOrEmpty(a.FailureReason))
                    .GroupBy(a => a.FailureReason!)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Attempts by hour
                statistics.AttemptsByHour = attempts
                    .GroupBy(a => a.AttemptDate.ToString("yyyy-MM-dd HH:00"))
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.Count());

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login audit statistics");
                throw;
            }
        }

        public async Task<List<FailedLoginSummaryDto>> GetFailedLoginSummaryAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? topCount = null)
        {
            try
            {
                var query = _unitOfWork.LoginAttempts.GetAll()
                    .Where(l => !l.IsSuccessful);

                if (startDate.HasValue)
                    query = query.Where(l => l.AttemptDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(l => l.AttemptDate <= endDate.Value.AddDays(1).AddTicks(-1));

                var failedAttempts = await query
                    .Include(l => l.Personnel)
                    .ToListAsync();

                var summary = failedAttempts
                    .GroupBy(l => l.Email.ToLower())
                    .Select(g => new FailedLoginSummaryDto
                    {
                        Email = g.First().Email,
                        PersonnelId = g.First().PersonnelId,
                        PersonnelName = g.First().Personnel?.FullName,
                        FailedAttemptCount = g.Count(),
                        LastFailedAttempt = g.Max(l => l.AttemptDate),
                        LastIpAddress = g.OrderByDescending(l => l.AttemptDate).First().IpAddress,
                        MostCommonFailureReason = g
                            .Where(l => !string.IsNullOrEmpty(l.FailureReason))
                            .GroupBy(l => l.FailureReason!)
                            .OrderByDescending(fg => fg.Count())
                            .FirstOrDefault()?.Key
                    })
                    .OrderByDescending(s => s.FailedAttemptCount)
                    .ThenByDescending(s => s.LastFailedAttempt);

                if (topCount.HasValue)
                    return summary.Take(topCount.Value).ToList();

                return summary.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting failed login summary");
                throw;
            }
        }
    }
}
