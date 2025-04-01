using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Filters
{
    public class LoggingFilter : IAsyncActionFilter
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _logger: İstekleri ve hataları loglamak için kullanıyorum.
        private readonly ILogger<LoggingFilter> _logger;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public LoggingFilter(ILogger<LoggingFilter> logger)
        {
            _logger = logger;
        }

        // Bu metodumla gelen istekleri ve yanıtları logluyorum.
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // İstekten önce bazı bilgileri topluyorum.
            var user = context.HttpContext.User.Identity?.Name ?? "Anonymous"; // Kullanıcı adını alıyorum, yoksa "Anonymous" kullanıyorum.
            var action = context.ActionDescriptor.DisplayName; // Hangi action'ın çalıştığını alıyorum.
            var method = context.HttpContext.Request.Method; // HTTP metodunu (GET, POST, vb.) alıyorum.
            var path = context.HttpContext.Request.Path; // İsteğin yolunu alıyorum.
            var query = context.HttpContext.Request.QueryString.ToString(); // Query parametrelerini alıyorum.

            // İsteği logluyorum.
            _logger.LogInformation($"İstek alındı: User: {user}, Method: {method}, Path: {path}, Query: {query}, Action: {action}");

            // Action'ı çalıştırıyorum ve sonucu bekliyorum.
            var resultContext = await next();

            // İstek tamamlandıktan sonra loglama yapıyorum.
            if (resultContext.Exception != null)
            {
                // Eğer bir hata olmuşsa, bunu logluyorum.
                _logger.LogError(resultContext.Exception, $"İstek sırasında hata çıktı: User: {user}, Path: {path}. InnerException: {resultContext.Exception.InnerException?.Message}");
            }
            else
            {
                // Hata yoksa, isteğin başarıyla tamamlandığını ve durum kodunu logluyorum.
                var statusCode = resultContext.HttpContext.Response.StatusCode;
                _logger.LogInformation($"İstek tamamlandı: User: {user}, Path: {path}, StatusCode: {statusCode}");
            }
        }
    }
}