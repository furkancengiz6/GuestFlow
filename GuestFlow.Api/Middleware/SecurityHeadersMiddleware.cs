using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Api.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly GuestFlow.Api.Configuration.SecurityHeadersSettings _settings;

        public SecurityHeadersMiddleware(
            RequestDelegate next,
            ILogger<SecurityHeadersMiddleware> logger,
            IWebHostEnvironment env,
            IOptions<GuestFlow.Api.Configuration.SecurityHeadersSettings> settings)
        {
            _next = next;
            _logger = logger;
            _env = env;
            _settings = settings.Value ?? new GuestFlow.Api.Configuration.SecurityHeadersSettings();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Build a CSP that doesn't break websocket connections (SignalR) and is dev-friendly.
            var connectSrc = new List<string>
            {
                "'self'",
                "https://cdn.jsdelivr.net",
                "ws:",
                "wss:",
            };

            // Production: allow the public API host explicitly (in addition to 'self').
            connectSrc.Add("https://api.guestflow.com");
            connectSrc.Add("wss://api.guestflow.com");

            // Development: allow local frontend/API ports (Vite + local API + websocket).
            if (_env.IsDevelopment())
            {
                connectSrc.Add("http://localhost:*");
                connectSrc.Add("https://localhost:*");
                connectSrc.Add("ws://localhost:*");
                connectSrc.Add("wss://localhost:*");
                connectSrc.Add("http://127.0.0.1:*");
                connectSrc.Add("https://127.0.0.1:*");
                connectSrc.Add("ws://127.0.0.1:*");
                connectSrc.Add("wss://127.0.0.1:*");
            }

            // Config-driven additions (per-environment via appsettings.*.json or env vars)
            if (_settings.ConnectSrc != null && _settings.ConnectSrc.Count > 0)
            {
                foreach (var entry in _settings.ConnectSrc.Where(x => !string.IsNullOrWhiteSpace(x)))
                    connectSrc.Add(entry.Trim());
            }

            // Content Security Policy - Enhanced for GuestFlow
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://unpkg.com; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
                "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                "img-src 'self' data: blob: https://cdn.jsdelivr.net https://*.guestflow.com; " +
                $"connect-src {string.Join(' ', connectSrc.Distinct())}; " +
                "frame-src 'none'; " +
                "object-src 'none'; " +
                "base-uri 'self'; " +
                "form-action 'self'; " +
                "frame-ancestors 'none'; " +
                "upgrade-insecure-requests;";

            // Security Headers
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=()";
            context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
            context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
            context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";

            // HSTS (HTTP Strict Transport Security) - Only for HTTPS
            if (context.Request.IsHttps)
            {
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            }

            // Remove server header for security
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");

            await _next(context);
        }
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}