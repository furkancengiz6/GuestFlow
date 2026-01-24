using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Auth
{
    /// <summary>
    /// Brute-force protection service implementation
    /// Implements rate limiting based on failed login attempts
    /// </summary>
    public class BruteForceProtectionService : IBruteForceProtectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BruteForceProtectionService> _logger;

        // Configuration defaults
        private readonly int _maxFailedAttempts;
        private readonly int _lockoutDurationMinutes;
        private readonly int _lockoutWindowMinutes;

        public BruteForceProtectionService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<BruteForceProtectionService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;

            // Get configuration values
            _maxFailedAttempts = int.TryParse(_configuration["Security:BruteForce:MaxFailedAttempts"], out var maxAttempts) 
                ? maxAttempts : 5; // Default: 5 failed attempts
            _lockoutDurationMinutes = int.TryParse(_configuration["Security:BruteForce:LockoutDurationMinutes"], out var lockoutDuration) 
                ? lockoutDuration : 15; // Default: 15 minutes lockout
            _lockoutWindowMinutes = int.TryParse(_configuration["Security:BruteForce:LockoutWindowMinutes"], out var window) 
                ? window : 15; // Default: 15 minutes window
        }

        public async Task<bool> IsLoginAllowedAsync(string email, string? ipAddress)
        {
            try
            {
                var windowStart = DateTime.UtcNow.AddMinutes(-_lockoutWindowMinutes);
                
                // Check failed attempts for email
                var emailFailedCount = await _unitOfWork.LoginAttempts
                    .GetAll(l => l.Email.ToLower() == email.ToLower() 
                        && !l.IsSuccessful 
                        && l.AttemptDate >= windowStart)
                    .CountAsync();

                if (emailFailedCount >= _maxFailedAttempts)
                {
                    // Check if still in lockout period
                    var lastFailedAttempt = await _unitOfWork.LoginAttempts
                        .GetAll(l => l.Email.ToLower() == email.ToLower() 
                            && !l.IsSuccessful 
                            && l.AttemptDate >= windowStart)
                        .OrderByDescending(l => l.AttemptDate)
                        .FirstOrDefaultAsync();

                    if (lastFailedAttempt != null)
                    {
                        var lockoutEnd = lastFailedAttempt.AttemptDate.AddMinutes(_lockoutDurationMinutes);
                        if (DateTime.UtcNow < lockoutEnd)
                        {
                            _logger.LogWarning($"Login blocked for email {email} - too many failed attempts");
                            return false;
                        }
                    }
                }

                // Check failed attempts for IP address (if provided)
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    var ipFailedCount = await _unitOfWork.LoginAttempts
                        .GetAll(l => l.IpAddress == ipAddress 
                            && !l.IsSuccessful 
                            && l.AttemptDate >= windowStart)
                        .CountAsync();

                    if (ipFailedCount >= _maxFailedAttempts * 2) // IP-based limit is higher (10 attempts)
                    {
                        var lastFailedAttempt = await _unitOfWork.LoginAttempts
                            .GetAll(l => l.IpAddress == ipAddress 
                                && !l.IsSuccessful 
                                && l.AttemptDate >= windowStart)
                            .OrderByDescending(l => l.AttemptDate)
                            .FirstOrDefaultAsync();

                        if (lastFailedAttempt != null)
                        {
                            var lockoutEnd = lastFailedAttempt.AttemptDate.AddMinutes(_lockoutDurationMinutes);
                            if (DateTime.UtcNow < lockoutEnd)
                            {
                                _logger.LogWarning($"Login blocked for IP {ipAddress} - too many failed attempts");
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking login allowance for {email}");
                // On error, allow login (fail open) to avoid blocking legitimate users
                return true;
            }
        }

        public async Task RecordLoginAttemptAsync(string email, string? ipAddress, bool isSuccessful, string? failureReason, int? personnelId = null)
        {
            try
            {
                var attempt = new LoginAttemptEntity
                {
                    Email = email,
                    IpAddress = ipAddress,
                    IsSuccessful = isSuccessful,
                    FailureReason = failureReason,
                    AttemptDate = DateTime.UtcNow,
                    PersonnelId = personnelId
                };

                await _unitOfWork.LoginAttempts.AddAsync(attempt);
                await _unitOfWork.SaveChangesAsync();

                if (isSuccessful)
                {
                    // Clear old failed attempts on successful login
                    await ClearFailedAttemptsAsync(email, ipAddress);
                }
                else
                {
                    _logger.LogWarning($"Failed login attempt: Email={email}, IP={ipAddress}, Reason={failureReason}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error recording login attempt for {email}");
                // Don't throw - logging failures shouldn't block login
            }
        }

        public async Task<int> GetRemainingLockoutTimeAsync(string email, string? ipAddress)
        {
            try
            {
                var windowStart = DateTime.UtcNow.AddMinutes(-_lockoutWindowMinutes);
                
                // Check email-based lockout
                var lastFailedAttempt = await _unitOfWork.LoginAttempts
                    .GetAll(l => l.Email.ToLower() == email.ToLower() 
                        && !l.IsSuccessful 
                        && l.AttemptDate >= windowStart)
                    .OrderByDescending(l => l.AttemptDate)
                    .FirstOrDefaultAsync();

                if (lastFailedAttempt != null)
                {
                    var failedCount = await _unitOfWork.LoginAttempts
                        .GetAll(l => l.Email.ToLower() == email.ToLower() 
                            && !l.IsSuccessful 
                            && l.AttemptDate >= windowStart)
                        .CountAsync();

                    if (failedCount >= _maxFailedAttempts)
                    {
                        var lockoutEnd = lastFailedAttempt.AttemptDate.AddMinutes(_lockoutDurationMinutes);
                        var remaining = (int)(lockoutEnd - DateTime.UtcNow).TotalSeconds;
                        return remaining > 0 ? remaining : 0;
                    }
                }

                // Check IP-based lockout
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    var lastFailedIpAttempt = await _unitOfWork.LoginAttempts
                        .GetAll(l => l.IpAddress == ipAddress 
                            && !l.IsSuccessful 
                            && l.AttemptDate >= windowStart)
                        .OrderByDescending(l => l.AttemptDate)
                        .FirstOrDefaultAsync();

                    if (lastFailedIpAttempt != null)
                    {
                        var ipFailedCount = await _unitOfWork.LoginAttempts
                            .GetAll(l => l.IpAddress == ipAddress 
                                && !l.IsSuccessful 
                                && l.AttemptDate >= windowStart)
                            .CountAsync();

                        if (ipFailedCount >= _maxFailedAttempts * 2)
                        {
                            var lockoutEnd = lastFailedIpAttempt.AttemptDate.AddMinutes(_lockoutDurationMinutes);
                            var remaining = (int)(lockoutEnd - DateTime.UtcNow).TotalSeconds;
                            return remaining > 0 ? remaining : 0;
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting lockout time for {email}");
                return 0;
            }
        }

        public async Task<int> GetFailedAttemptCountAsync(string email, string? ipAddress)
        {
            try
            {
                var windowStart = DateTime.UtcNow.AddMinutes(-_lockoutWindowMinutes);
                
                var emailCount = await _unitOfWork.LoginAttempts
                    .GetAll(l => l.Email.ToLower() == email.ToLower() 
                        && !l.IsSuccessful 
                        && l.AttemptDate >= windowStart)
                    .CountAsync();

                return emailCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting failed attempt count for {email}");
                return 0;
            }
        }

        public async Task ClearFailedAttemptsAsync(string email, string? ipAddress)
        {
            try
            {
                // Note: We don't delete old attempts for audit purposes
                // Instead, we rely on time-based queries that ignore old attempts
                // This method is kept for future use if needed
                _logger.LogInformation($"Cleared failed attempts tracking for {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clearing failed attempts for {email}");
            }
        }
    }
}
