using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuestFlow.Api.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Bir sonraki middleware veya endpoint'i çalıştırıyorum.
                await _next(context);
            }
            catch (Exception ex)
            {
                // Hata yakalandığında logluyorum ve istemciye uygun bir yanıt döndürüyorum.
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Hata detaylarını logluyorum.
            _logger.LogError(exception, $"Bir hata oluştu: {exception.Message}. InnerException: {exception.InnerException?.Message}");

            // Hata mesajını ve durum kodunu belirliyorum.
            var statusCode = HttpStatusCode.InternalServerError; // Varsayılan olarak 500
            var message = "Bir hata oluştu, lütfen daha sonra tekrar deneyin.";

            // Hatanın türüne göre özelleştirilmiş yanıtlar oluşturuyorum.
            switch (exception)
            {
                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest; // 400
                    message = argEx.Message;
                    break;
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized; // 401
                    message = "Yetkisiz erişim.";
                    break;
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound; // 404
                    message = "İstenen kaynak bulunamadı.";
                    break;
                    // Diğer hata türleri için ek case'ler eklenebilir.
            }

            // Yanıtın türünü JSON olarak ayarlıyorum.
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // Hata mesajını JSON formatında istemciye gönderiyorum.
            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = message,
                Detailed = exception.Message // Geliştirme ortamında detaylı hata mesajı, üretimde gizlenebilir.
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    // Extension metodu ile middleware'i kolayca ekleyebilmek için.
    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}