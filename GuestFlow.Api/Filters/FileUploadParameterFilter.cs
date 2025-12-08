using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace GuestFlow.Api.Filters
{
    /// <summary>
    /// Swagger'da IFormFile parametrelerini parametre listesinden kaldırmak için filter
    /// Bu filter, Swagger'ın parametre okuma aşamasında hatayı önler
    /// </summary>
    public class FileUploadParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            try
            {
                // IFormFile parametrelerini parametre listesinden kaldır
                // Çünkü bunlar RequestBody içinde işlenecek
                if (context.ParameterInfo != null)
                {
                    var paramType = context.ParameterInfo.ParameterType;
                    if (IsFormFileType(paramType))
                    {
                        // Bu parametreyi Swagger'dan tamamen kaldır
                        // In property'si nullable değil, bu yüzden sadece schema'yı değiştiriyoruz
                        parameter.Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        };
                        // In property'sini değiştirmeye çalışmayız, çünkü nullable değil
                        // OperationFilter bunu RequestBody'ye taşıyacak
                    }
                }
            }
            catch
            {
                // Hata durumunda filter'ı atla
            }
        }

        private bool IsFormFileType(System.Type type)
        {
            if (type == null)
                return false;

            if (type == typeof(IFormFile))
                return true;

            if (type == typeof(IFormFile[]))
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>) &&
                type.GetGenericArguments().Length > 0 &&
                type.GetGenericArguments()[0] == typeof(IFormFile))
                return true;

            return false;
        }
    }
}

