using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

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
                        var sanitizedBody = SanitizeRequestBody(context, body);

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

        private string SanitizeRequestBody(HttpContext context, string body)
        {
            // JSON-safe sanitization: parse JSON and sanitize only string values so we keep valid JSON
            var contentType = context.Request.ContentType ?? string.Empty;
            if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var node = JsonNode.Parse(body);
                    if (node is null)
                        return body;

                    SanitizeJsonNodeStrings(node);
                    return node.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = false
                    });
                }
                catch (JsonException)
                {
                    // Not valid JSON; fall back to HTML sanitization to avoid breaking the request pipeline.
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "JSON sanitization failed - falling back to HTML sanitization");
                }
            }

            return SanitizeHtml(body);
        }

        private void SanitizeJsonNodeStrings(JsonNode node)
        {
            switch (node)
            {
                case JsonObject obj:
                    foreach (var kvp in obj.ToList())
                    {
                        var key = kvp.Key;
                        var child = kvp.Value;
                        if (child is null) continue;

                        if (child is JsonValue value && value.TryGetValue<string>(out var s))
                        {
                            obj[key] = SanitizeHtml(s);
                        }
                        else
                        {
                            SanitizeJsonNodeStrings(child);
                        }
                    }
                    break;

                case JsonArray arr:
                    for (var i = 0; i < arr.Count; i++)
                    {
                        var child = arr[i];
                        if (child is null) continue;

                        if (child is JsonValue value && value.TryGetValue<string>(out var s))
                        {
                            arr[i] = SanitizeHtml(s);
                        }
                        else
                        {
                            SanitizeJsonNodeStrings(child);
                        }
                    }
                    break;
            }
        }

        private string SanitizeHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            try
            {
                // Use HtmlSanitizer for robust sanitization
                var sanitizer = new Ganss.Xss.HtmlSanitizer();

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