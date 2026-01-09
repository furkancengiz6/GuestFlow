using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace GuestFlow.Api.Middleware
{
    public class HtmlSanitizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HtmlSanitizationMiddleware> _logger;

        public HtmlSanitizationMiddleware(RequestDelegate next, ILogger<HtmlSanitizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only sanitize POST, PUT, PATCH requests
            if (HttpMethods.IsPost(context.Request.Method) ||
                HttpMethods.IsPut(context.Request.Method) ||
                HttpMethods.IsPatch(context.Request.Method))
            {
                try
                {
                    // Enable buffering to read request body
                    context.Request.EnableBuffering();

                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();

                    if (!string.IsNullOrEmpty(body))
                    {
                        var sanitizedBody = SanitizeHtml(body);

                        // Reset stream position and replace body
                        var buffer = Encoding.UTF8.GetBytes(sanitizedBody);
                        context.Request.Body = new MemoryStream(buffer);
                        context.Request.Body.Position = 0;

                        // Log if content was modified
                        if (!body.Equals(sanitizedBody, StringComparison.Ordinal))
                        {
                            _logger.LogWarning("HTML content sanitized in request to {Path}", context.Request.Path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during HTML sanitization for request {Path}", context.Request.Path);
                    // Continue processing even if sanitization fails
                }
            }

            await _next(context);
        }

        private string SanitizeHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            try
            {
                // Use Ganss.XSS HtmlSanitizer for robust sanitization
                var sanitizer = new Ganss.XSS.HtmlSanitizer();

                // Configure allowed tags conservatively
                sanitizer.AllowedTags.Clear();
                var allowed = new[] { "p", "br", "strong", "b", "em", "i", "u", "ul", "ol", "li", "h1", "h2", "h3", "h4", "h5", "h6", "table", "thead", "tbody", "tr", "th", "td", "a" };
                foreach (var tag in allowed) sanitizer.AllowedTags.Add(tag);

                // Allow safe attributes on links and basic attributes
                sanitizer.AllowedAttributes.Clear();
                sanitizer.AllowedAttributes.Add("href");
                sanitizer.AllowedAttributes.Add("title");
                sanitizer.AllowedAttributes.Add("class");
                sanitizer.AllowedAttributes.Add("id");

                // Remove unsafe CSS properties if present
                try
                {
                    sanitizer.AllowedCssProperties.Remove("position");
                    sanitizer.AllowedCssProperties.Remove("z-index");
                }
                catch { /* ignore if property collection not available */ }

                var result = sanitizer.Sanitize(html);
                return result ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SanitizeHtml failed - falling back to basic encoding");
                return WebUtility.HtmlEncode(html);
            }
        }
    }

    public static class HtmlSanitizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseHtmlSanitization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HtmlSanitizationMiddleware>();
        }
    }
}