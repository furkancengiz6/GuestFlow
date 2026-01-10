using GuestFlow.Application.Operations.Setting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Api.Middlewares
{
    public class MaintenanceMiddleware
    {
        // Burada kullanacağım değişkenleri tanımlıyorum.
        // _next: Bir sonraki middleware'e geçiş yapmak için kullanıyorum.
        // _logger: Hataları veya bilgileri loglamak için kullanıyorum.
        private readonly RequestDelegate _next;
        private readonly ILogger<MaintenanceMiddleware> _logger;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public MaintenanceMiddleware(RequestDelegate next, ILogger<MaintenanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Bu metodumla gelen HTTP isteklerini kontrol ediyorum.
        public async Task InvokeAsync(HttpContext context)
        {
            // Önce, gelen isteğin yolunu kontrol ediyorum.
            // Eğer istek login/settings gibi "erişilebilir kalması gereken" bir yola gidiyorsa, bakım modundan muaf tutuyorum.
            // Not: API endpoint'leri URL segment versioning kullanıyor: /api/v1.0/...
            if (IsExemptFromMaintenance(context.Request.Path))
            {
                // Bu yollar için direkt bir sonraki middleware'e geçiyorum.
                await _next(context);
                return;
            }

            // Bakım modunun aktif olup olmadığını kontrol etmek için ISettingsService'i alıyorum.
            var settingService = context.RequestServices.GetRequiredService<ISettingsService>();
            bool maintenanceMode = await settingService.GetMaintenanceState();

            // Eğer bakım modu aktifse, isteği reddediyorum.
            if (maintenanceMode)
            {
                // Bakım modunun aktif olduğunu ve isteğin reddedildiğini logluyorum.
                _logger.LogWarning($"Bakım modu aktif, istek reddedildi: {context.Request.Path}");
                // HTTP durum kodunu 503 (Service Unavailable) olarak ayarlıyorum.
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                // Yanıtın türünü JSON olarak belirtiyorum.
                context.Response.ContentType = "application/json";
                // Kullanıcıya bir hata mesajı gönderiyorum.
                await context.Response.WriteAsync("{\"message\": \"Şu anda hizmet verememekteyiz.\"}");
            }
            else
            {
                // Bakım modu aktif değilse, bir sonraki middleware'e geçiyorum.
                await _next(context);
            }
        }

        private static bool IsExemptFromMaintenance(PathString path)
        {
            var raw = path.Value ?? string.Empty;
            if (!raw.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
                return false;

            // Normalize: allow both /api/auth/... and /api/v{version}/auth/... patterns.
            // Example segments:
            // - /api/auth/login              -> ["api","auth","login"]
            // - /api/v1.0/auth/login         -> ["api","v1.0","auth","login"]
            var segments = raw.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
                return false;

            // segments[0] == "api"
            var index = 1;
            if (segments.Length > 2 && segments[1].StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                index = 2;
            }

            var remainder = string.Join('/', segments.Skip(index));
            return remainder.StartsWith("auth/login", StringComparison.OrdinalIgnoreCase) ||
                   remainder.StartsWith("auth/settings", StringComparison.OrdinalIgnoreCase) ||
                   remainder.StartsWith("settings", StringComparison.OrdinalIgnoreCase);
        }
    }
}