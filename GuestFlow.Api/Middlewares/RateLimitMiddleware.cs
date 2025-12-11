using GuestFlow.Application.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

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

        public RateLimitMiddleware(
            RequestDelegate next,
            ILogger<RateLimitMiddleware> logger,
            IOptions<RateLimitSettings> settings,
            IMemoryCache cache)
        {
            _next = next;
            _logger = logger;
            _settings = settings.Value;
            _cache = cache;
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
            var endpoint = context.Request.Path.Value ?? string.Empty;

            // Endpoint bazlı limit kontrolü
            var limit = GetEndpointLimit(endpoint);

            // Per minute kontrolü
            var minuteKey = $"ratelimit:{clientIp}:{endpoint}:minute:{DateTime.UtcNow:yyyy-MM-dd-HH-mm}";
            var minuteCount = _cache.Get<int?>(minuteKey) ?? 0;

            if (minuteCount >= limit.RequestsPerMinute)
            {
                _logger.LogWarning($"Rate limit exceeded (per minute) for IP: {clientIp}, Endpoint: {endpoint}");
                await ReturnRateLimitResponse(context, limit.RequestsPerMinute, "minute");
                return;
            }

            // Per hour kontrolü
            var hourKey = $"ratelimit:{clientIp}:{endpoint}:hour:{DateTime.UtcNow:yyyy-MM-dd-HH}";
            var hourCount = _cache.Get<int?>(hourKey) ?? 0;

            if (hourCount >= limit.RequestsPerHour)
            {
                _logger.LogWarning($"Rate limit exceeded (per hour) for IP: {clientIp}, Endpoint: {endpoint}");
                await ReturnRateLimitResponse(context, limit.RequestsPerHour, "hour");
                return;
            }

            // Cache'e sayacı ekle/güncelle
            _cache.Set(minuteKey, minuteCount + 1, TimeSpan.FromMinutes(1));
            _cache.Set(hourKey, hourCount + 1, TimeSpan.FromHours(1));

            // İsteği devam ettir
            await _next(context);
        }

        private bool IsWhitelisted(PathString path)
        {
            return _settings.WhitelistedPaths.Any(whitelisted => 
                path.StartsWithSegments(whitelisted, StringComparison.OrdinalIgnoreCase));
        }

        private EndpointRateLimit GetEndpointLimit(string endpoint)
        {
            // Endpoint bazlı özel limit var mı kontrol et
            foreach (var kvp in _settings.EndpointLimits)
            {
                if (endpoint.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            // Varsayılan limit
            return new EndpointRateLimit
            {
                RequestsPerMinute = _settings.DefaultRequestsPerMinute,
                RequestsPerHour = _settings.DefaultRequestsPerHour
            };
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

