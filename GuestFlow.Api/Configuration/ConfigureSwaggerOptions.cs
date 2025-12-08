using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GuestFlow.Api.Configuration
{
    /// <summary>
    /// Swagger yapılandırması - Her API versiyonu için ayrı dokümantasyon oluşturur
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Her API versiyonu için Swagger dokümantasyonu oluşturur
        /// </summary>
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        /// <summary>
        /// Belirli bir isim için yapılandırma (IConfigureNamedOptions interface'i için)
        /// </summary>
        public void Configure(string? name, SwaggerGenOptions options)
        {
            Configure(options);
        }

        /// <summary>
        /// API versiyonu için OpenAPI bilgilerini oluşturur
        /// </summary>
        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = "GuestFlow API",
                Version = description.ApiVersion.ToString(),
                Description = "GuestFlow Otel ve Misafir Yönetim Sistemi API Dokümantasyonu",
                Contact = new OpenApiContact
                {
                    Name = "GuestFlow Support",
                    Email = "support@guestflow.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            };

            if (description.IsDeprecated)
            {
                info.Description += " <strong>Bu API versiyonu kullanımdan kaldırılmıştır (Deprecated).</strong>";
            }

            return info;
        }
    }
}

