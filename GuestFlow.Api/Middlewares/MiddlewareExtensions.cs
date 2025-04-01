namespace GuestFlow.Api.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMantenanceMode(this IApplicationBuilder app) {
            return app.UseMiddleware<MaintenanceMiddleware>();
        }
    }
}
