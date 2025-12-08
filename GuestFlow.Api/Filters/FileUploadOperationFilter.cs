using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GuestFlow.Api.Filters
{
    /// <summary>
    /// Swagger'da IFormFile parametrelerini doğru şekilde göstermek için filter
    /// [FromForm] attribute'u olan veya IFormFile tipinde olan tüm parametreleri multipart/form-data olarak işler
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            try
            {
                if (context?.MethodInfo == null)
                    return;

                // Tüm parametreleri al
                var allParameters = context.MethodInfo.GetParameters()?.ToList();
                if (allParameters == null || !allParameters.Any())
                    return;
                
                // IFormFile parametreleri var mı kontrol et
                var hasFormFile = allParameters.Any(p => IsFormFileType(p.ParameterType));
                
                // [FromForm] attribute'u olan parametreler var mı kontrol et
                var hasFromForm = allParameters.Any(p => 
                    p.GetCustomAttributes(typeof(FromFormAttribute), false).Any());

                // [Consumes("multipart/form-data")] attribute'u var mı kontrol et
                var consumesMultipart = false;
                try
                {
                    var consumesAttributes = context.MethodInfo.GetCustomAttributes(typeof(ConsumesAttribute), false)
                        .Cast<ConsumesAttribute>();
                    consumesMultipart = consumesAttributes.Any(a => 
                        a?.ContentTypes != null && 
                        a.ContentTypes.Any(ct => ct != null && ct.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase)));
                }
                catch
                {
                    consumesMultipart = false;
                }

                // Eğer IFormFile yoksa ve multipart/form-data consume edilmiyorsa, filter'ı uygulama
                if (!hasFormFile && !hasFromForm && !consumesMultipart)
                    return;

                // Önce mevcut parametreleri temizle (IFormFile olanları kaldır)
                if (operation.Parameters != null)
                {
                    var formFileParams = operation.Parameters
                        .Where(p => p.Name != null && 
                                   (p.Name.Contains("file", StringComparison.OrdinalIgnoreCase) ||
                                    p.Schema?.Format == "binary"))
                        .ToList();
                    
                    foreach (var param in formFileParams)
                    {
                        operation.Parameters.Remove(param);
                    }
                }

                // RequestBody oluştur veya güncelle
                if (operation.RequestBody == null)
                {
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Content = new Dictionary<string, OpenApiMediaType>()
                    };
                }

                if (!operation.RequestBody.Content.ContainsKey("multipart/form-data"))
                {
                    operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>(),
                            Required = new HashSet<string>()
                        }
                    };
                }

                var schema = operation.RequestBody.Content["multipart/form-data"].Schema;
                if (schema.Properties == null)
                    schema.Properties = new Dictionary<string, OpenApiSchema>();
                if (schema.Required == null)
                    schema.Required = new HashSet<string>();

                // Tüm parametreleri işle
                // Önce [FromForm] attribute'u olanları, sonra IFormFile tipinde olanları al
                var formParameters = allParameters
                    .Where(p => 
                        p.GetCustomAttributes(typeof(FromFormAttribute), false).Any() ||
                        IsFormFileType(p.ParameterType) ||
                        (consumesMultipart && !p.GetCustomAttributes(typeof(FromBodyAttribute), false).Any() &&
                         !p.GetCustomAttributes(typeof(FromQueryAttribute), false).Any() &&
                         !p.GetCustomAttributes(typeof(FromRouteAttribute), false).Any() &&
                         !p.GetCustomAttributes(typeof(FromHeaderAttribute), false).Any()))
                    .ToList();

                foreach (var parameter in formParameters)
                {
                    if (string.IsNullOrEmpty(parameter.Name))
                        continue;

                    OpenApiSchema paramSchema;

                    if (IsFormFileType(parameter.ParameterType))
                    {
                        // IFormFile için
                        if (parameter.ParameterType == typeof(IFormFile))
                        {
                            paramSchema = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary",
                                Description = "Dosya yükleme"
                            };
                        }
                        else if (parameter.ParameterType == typeof(IFormFile[]))
                        {
                            // IFormFile[]
                            paramSchema = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                },
                                Description = "Dosya listesi"
                            };
                        }
                        else
                        {
                            // List<IFormFile>
                            paramSchema = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                },
                                Description = "Dosya listesi"
                            };
                        }
                    }
                    else
                    {
                        // Diğer form parametreleri
                        paramSchema = GetOpenApiSchema(parameter.ParameterType);
                    }

                    schema.Properties[parameter.Name] = paramSchema;

                    // Required kontrolü
                    // IFormFile ve nullable olmayan, default value olmayan parametreler required
                    var isRequired = IsFormFileType(parameter.ParameterType) && !parameter.HasDefaultValue;
                    
                    if (!isRequired)
                    {
                        // Diğer parametreler için: default value yoksa ve nullable değilse required
                        isRequired = !parameter.HasDefaultValue && 
                                     !IsNullableType(parameter.ParameterType) &&
                                     parameter.ParameterType != typeof(string) &&
                                     Nullable.GetUnderlyingType(parameter.ParameterType) == null;
                    }
                    
                    if (isRequired && !schema.Required.Contains(parameter.Name))
                    {
                        schema.Required.Add(parameter.Name);
                    }
                }

                // Eski parametreleri kaldır (artık RequestBody'de)
                if (operation.Parameters != null)
                {
                    var paramsToRemove = operation.Parameters
                        .Where(p => formParameters.Any(fp => fp.Name == p.Name))
                        .ToList();
                    
                    foreach (var param in paramsToRemove)
                    {
                        operation.Parameters.Remove(param);
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda filter'ı atla, Swagger'ın varsayılan davranışını kullan
                // Production'da loglama yapılabilir
                System.Diagnostics.Debug.WriteLine($"FileUploadOperationFilter hatası: {ex.Message}");
            }
        }

        private bool IsFormFileType(Type type)
        {
            if (type == typeof(IFormFile))
                return true;

            if (type == typeof(IFormFile[]))
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) &&
                type.GetGenericArguments()[0] == typeof(IFormFile))
                return true;

            return false;
        }

        private bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private OpenApiSchema GetOpenApiSchema(Type type)
        {
            try
            {
                if (type == null)
                    return new OpenApiSchema { Type = "string" };

                // Nullable type'ları handle et
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

                var schema = new OpenApiSchema();

                if (underlyingType == typeof(string))
                {
                    schema.Type = "string";
                }
                else if (underlyingType == typeof(int) || underlyingType == typeof(long))
                {
                    schema.Type = "integer";
                    schema.Format = underlyingType == typeof(long) ? "int64" : "int32";
                }
                else if (underlyingType == typeof(bool))
                {
                    schema.Type = "boolean";
                }
                else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
                {
                    schema.Type = "number";
                    schema.Format = underlyingType == typeof(double) ? "double" : "float";
                }
                else if (underlyingType == typeof(DateTime))
                {
                    schema.Type = "string";
                    schema.Format = "date-time";
                }
                else if (underlyingType.IsEnum)
                {
                    schema.Type = "string";
                }
                else
                {
                    // Complex type'lar için varsayılan olarak string
                    schema.Type = "string";
                }

                return schema;
            }
            catch
            {
                // Hata durumunda varsayılan schema döndür
                return new OpenApiSchema { Type = "string" };
            }
        }

    }
}

