using GuestFlow.Application.Operations.Setting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GuestFlow.Api.Middlewares
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MaintenanceMiddleware> _logger;

        public MaintenanceMiddleware(RequestDelegate next, ILogger<MaintenanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // /api/auth/login, /api/auth/settings ve /api/settings yollarını bakım modundan muaf tut
            if (context.Request.Path.StartsWithSegments("/api/auth/login") ||
                context.Request.Path.StartsWithSegments("/api/auth/settings") ||
                context.Request.Path.StartsWithSegments("/api/settings"))
            {
                await _next(context);
                return;
            }

            var settingService = context.RequestServices.GetRequiredService<ISettingsService>();
            bool maintenanceMode = await settingService.GetMaintenanceState();

            if (maintenanceMode)
            {
                _logger.LogWarning("Bakım modu aktif, istek reddedildi: {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\": \"Şu anda hizmet verememekteyiz.\"}");
            }
            else
            {
                await _next(context);
            }
        }
    }
}