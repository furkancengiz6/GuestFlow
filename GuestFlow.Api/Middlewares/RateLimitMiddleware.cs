using GuestFlow.Application.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Security.Claims;

namespace GuestFlow.Api.Middlewares
{
    /// <summary>
    /// Rate limiting middleware - IP bazlı istek sınırlama
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly RateLimitSettings _settings;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;

        public RateLimitMiddleware(
            RequestDelegate next,
            ILogger<RateLimitMiddleware> logger,
            IOptions<RateLimitSettings> settings,
            IMemoryCache cache,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _settings = settings.Value;
            _cache = cache;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Rate limiting devre dışıysa veya whitelist'te ise geç
            if (!_settings.Enabled || IsWhitelisted(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var clientIp = GetClientIpAddress(context);
            // In development, allow loopback traffic to bypass rate limiting (helps local E2E/test runners)
            if (_env.IsDevelopment() && (clientIp == "127.0.0.1" || clientIp == "::1" || clientIp.StartsWith("::ffff:127.0.0.1")))
            {
                await _next(context);
                return;
            }
            var endpoint = context.Request.Path.Value ?? string.Empty;
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var method = context.Request.Method;
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            // SECURITY: Basic IP blocking for known malicious IPs
            if (IsBlockedIp(clientIp))
            {
                _logger.LogWarning($"Blocked request from blacklisted IP: {clientIp}");
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Access denied");
                return;
            }

            // SECURITY: User-Agent validation (configurable bot detection)
            // Dev/QA ergonomics: only enforce User-Agent blocking in Production (optional).
            // NOTE: Rate limiting already mitigates abuse; UA blocking should be reserved for clearly malicious scanners.
            if (_env.IsProduction() && IsBlockedUserAgent(userAgent))
            {
                _logger.LogWarning($"Blocked request from IP: {clientIp}, User-Agent: {userAgent}");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Invalid request");
                return;
            }

            // Check if user is blocked
            if (_settings.EnableUserBlocking && IsBlockedUser(userId))
            {
                _logger.LogWarning($"Blocked request from blocked user: {userId}, IP: {clientIp}");
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Access denied");
                return;
            }

            // Endpoint bazlı limit kontrolü
            var limit = GetEndpointLimit(endpoint);

            // Check if IP is currently blocked
            if (_settings.EnableIpBlocking && IsCurrentlyBlocked(clientIp))
            {
                _logger.LogWarning($"Request blocked from temporarily blocked IP: {clientIp}");
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = _settings.BlockDurationMinutes.ToString();
                await context.Response.WriteAsync("Temporarily blocked due to rate limiting");
                return;
            }

            // Per minute kontrolü
            var minuteKey = $"ratelimit:{clientIp}:{endpoint}:minute:{DateTime.UtcNow:yyyy-MM-dd-HH-mm}";
            var minuteCount = _cache.Get<int?>(minuteKey) ?? 0;

            // Per hour kontrolü
            var hourKey = $"ratelimit:{clientIp}:{endpoint}:hour:{DateTime.UtcNow:yyyy-MM-dd-HH}";
            var hourCount = _cache.Get<int?>(hourKey) ?? 0;

            // Per day kontrolü
            var dayKey = $"ratelimit:{clientIp}:{endpoint}:day:{DateTime.UtcNow:yyyy-MM-dd}";
            var dayCount = _cache.Get<int?>(dayKey) ?? 0;

            // User-based limits (if user is authenticated)
            var userMinuteKey = $"ratelimit:user:{userId}:{endpoint}:minute:{DateTime.UtcNow:yyyy-MM-dd-HH-mm}";
            var userHourKey = $"ratelimit:user:{userId}:{endpoint}:hour:{DateTime.UtcNow:yyyy-MM-dd-HH}";
            var userDayKey = $"ratelimit:user:{userId}:{endpoint}:day:{DateTime.UtcNow:yyyy-MM-dd}";
            var userMinuteCount = _cache.Get<int?>(userMinuteKey) ?? 0;
            var userHourCount = _cache.Get<int?>(userHourKey) ?? 0;
            var userDayCount = _cache.Get<int?>(userDayKey) ?? 0;

            // Rate limit checks
            if (minuteCount >= limit.RequestsPerMinute ||
                (_settings.EnableUserBlocking && userId != "anonymous" && userMinuteCount >= limit.RequestsPerMinute))
            {
                await HandleRateLimitExceeded(context, clientIp, userId, endpoint, limit.RequestsPerMinute, "minute");
                return;
            }

            if (hourCount >= limit.RequestsPerHour ||
                (_settings.EnableUserBlocking && userId != "anonymous" && userHourCount >= limit.RequestsPerHour))
            {
                await HandleRateLimitExceeded(context, clientIp, userId, endpoint, limit.RequestsPerHour, "hour");
                return;
            }

            if (dayCount >= (limit.RequestsPerDay ?? _settings.DefaultRequestsPerDay) ||
                (_settings.EnableUserBlocking && userId != "anonymous" && userDayCount >= (limit.RequestsPerDay ?? _settings.DefaultRequestsPerDay)))
            {
                await HandleRateLimitExceeded(context, clientIp, userId, endpoint, limit.RequestsPerDay ?? _settings.DefaultRequestsPerDay, "day");
                return;
            }

            // Cache'e sayacı ekle/güncelle
            _cache.Set(minuteKey, minuteCount + 1, TimeSpan.FromMinutes(1));
            _cache.Set(hourKey, hourCount + 1, TimeSpan.FromHours(1));
            _cache.Set(dayKey, dayCount + 1, TimeSpan.FromDays(1));

            if (_settings.EnableUserBlocking && userId != "anonymous")
            {
                _cache.Set(userMinuteKey, userMinuteCount + 1, TimeSpan.FromMinutes(1));
                _cache.Set(userHourKey, userHourCount + 1, TimeSpan.FromHours(1));
                _cache.Set(userDayKey, userDayCount + 1, TimeSpan.FromDays(1));
            }

            // İsteği devam ettir
            await _next(context);
        }

        private bool IsWhitelisted(PathString path)
        {
            // Always whitelist internal development helper endpoint
            if (path.StartsWithSegments("/api/dev", StringComparison.OrdinalIgnoreCase))
                return true;

            return _settings.WhitelistedPaths.Any(whitelisted =>
                path.StartsWithSegments(whitelisted, StringComparison.OrdinalIgnoreCase));
        }

        private EndpointRateLimit GetEndpointLimit(string endpoint)
        {
            // Endpoint bazlı özel limit var mı kontrol et.
            // Supports both versioned and unversioned routes by trying a normalized variant:
            // - /api/v1.0/auth/login -> /api/auth/login
            // We also pick the *most specific* match (longest key) to avoid config ordering issues.
            var candidates = new[] { endpoint, NormalizeEndpointPath(endpoint) }
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            EndpointRateLimit? best = null;
            var bestLen = -1;

            foreach (var candidate in candidates)
            {
                foreach (var kvp in _settings.EndpointLimits)
                {
                    if (candidate.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase) && kvp.Key.Length > bestLen)
                    {
                        best = kvp.Value;
                        bestLen = kvp.Key.Length;
                    }
                }
            }

            if (best != null)
                return best;

            // Varsayılan limit
            return new EndpointRateLimit
            {
                RequestsPerMinute = _settings.DefaultRequestsPerMinute,
                RequestsPerHour = _settings.DefaultRequestsPerHour
            };
        }

        private static string NormalizeEndpointPath(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return endpoint;

            // /api/v1.0/...  => /api/...
            var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 3 &&
                segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) &&
                segments[1].StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                // Remove version segment ("v1", "v1.0", "v2.0", etc.)
                var normalized = new[] { segments[0] }.Concat(segments.Skip(2));
                return "/" + string.Join('/', normalized);
            }

            return endpoint;
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // X-Forwarded-For header'ını kontrol et (proxy/load balancer arkasında)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            // X-Real-IP header'ını kontrol et
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp.Trim();
            }

            // Remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private bool IsBlockedIp(string ipAddress)
        {
            // In development allow loopback traffic (don't block localhost)
            if (_env.IsDevelopment())
                return false;

            // SECURITY: Basic IP blacklist (production)
            var blockedIps = Array.Empty<string>(); // Add known malicious IPs here

            return blockedIps.Contains(ipAddress);
        }

        private bool IsBlockedUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return false; // don't hard-fail missing UA; rely on rate limiting instead

            // SECURITY: Configurable blocked User-Agent patterns
            if (_settings.BlockedUserAgents == null || !_settings.BlockedUserAgents.Any())
                return false;

            var lowerUserAgent = userAgent.ToLowerInvariant();
            return _settings.BlockedUserAgents.Any(pattern =>
                lowerUserAgent.Contains(pattern.ToLowerInvariant()));
        }

        private bool IsBlockedUser(string userId)
        {
            // Check if user is in blocked list (could be extended to check database)
            var blockedUsersKey = $"blocked:users";
            var blockedUsers = _cache.Get<HashSet<string>>(blockedUsersKey) ?? new HashSet<string>();
            return blockedUsers.Contains(userId);
        }

        private bool IsCurrentlyBlocked(string clientIp)
        {
            // In development do not treat loopback as blocked to avoid blocking local test runners
            if (_env.IsDevelopment())
            {
                if (clientIp == "127.0.0.1" || clientIp == "::1" || clientIp.StartsWith("::ffff:127.0.0.1"))
                    return false;
            }

            // Check if IP is temporarily blocked
            var blockKey = $"blocked:ip:{clientIp}";
            return _cache.TryGetValue(blockKey, out _);
        }

        private async Task HandleRateLimitExceeded(HttpContext context, string clientIp, string userId, string endpoint, int limit, string period)
        {
            _logger.LogWarning($"Rate limit exceeded ({period}) for IP: {clientIp}, User: {userId}, Endpoint: {endpoint}");

            // Temporarily block IP if configured
            if (_settings.EnableIpBlocking)
            {
                var blockKey = $"blocked:ip:{clientIp}";
                _cache.Set(blockKey, true, TimeSpan.FromMinutes(_settings.BlockDurationMinutes));
                _logger.LogWarning($"IP {clientIp} temporarily blocked for {_settings.BlockDurationMinutes} minutes");
            }

            await ReturnRateLimitResponse(context, limit, period);
        }

        private async Task ReturnRateLimitResponse(HttpContext context, int limit, string period)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message = $"Rate limit exceeded. Maximum {limit} requests per {period} allowed.",
                statusCode = 429,
                timestamp = DateTime.UtcNow,
                retryAfter = period == "minute" ? 60 : 3600
            };

            // Retry-After header ekle
            context.Response.Headers["Retry-After"] = period == "minute" ? "60" : "3600";
            context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
            context.Response.Headers["X-RateLimit-Period"] = period;

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}

