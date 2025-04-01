using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Filters
{
    public class LoggingFilter : IAsyncActionFilter
    {
        private readonly ILogger<LoggingFilter> _logger;

        public LoggingFilter(ILogger<LoggingFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // İstek öncesi loglama
            var user = context.HttpContext.User.Identity?.Name ?? "Anonymous";
            var action = context.ActionDescriptor.DisplayName;
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path;
            var query = context.HttpContext.Request.QueryString.ToString();

            _logger.LogInformation("İstek alındı: User={User}, Method={Method}, Path={Path}, Query={Query}, Action={Action}",
                user, method, path, query, action);

            // Action'ı çalıştır
            var resultContext = await next();

            // İstek sonrası loglama
            if (resultContext.Exception != null)
            {
                _logger.LogError(resultContext.Exception, "İstek sırasında hata oluştu: User={User}, Path={Path}", user, path);
            }
            else
            {
                var statusCode = resultContext.HttpContext.Response.StatusCode;
                _logger.LogInformation("İstek tamamlandı: User={User}, Path={Path}, StatusCode={StatusCode}", user, path, statusCode);
            }
        }
    }
}