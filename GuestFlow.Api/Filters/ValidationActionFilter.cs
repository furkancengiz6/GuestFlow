using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Api.Filters
{
    /// <summary>
    /// Model validasyonu için Action Filter
    /// FluentValidation ve ModelState hatalarını yakalar ve standart formatta döndürür
    /// </summary>
    public class ValidationActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<ValidationActionFilter> _logger;

        public ValidationActionFilter(ILogger<ValidationActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Action çalışmadan önce model validasyonunu kontrol et
            if (!context.ModelState.IsValid)
            {
                var errors = FormatValidationErrors(context.ModelState);
                
                _logger.LogWarning($"Model validasyon hatası: {context.ActionDescriptor.DisplayName}. Hatalar: {string.Join(", ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Messages)}"))}");

                context.Result = new BadRequestObjectResult(new ValidationErrorResponse
                {
                    Success = false,
                    Message = "Validasyon hatası: Lütfen gönderilen verileri kontrol ediniz.",
                    Errors = errors,
                    Timestamp = DateTime.UtcNow
                });

                return;
            }

            // Model geçerliyse action'ı çalıştır
            var executedContext = await next();

            // Action sonrası kontrol (eğer action içinde ModelState'e hata eklenmişse)
            if (executedContext.Result is BadRequestObjectResult badRequestResult)
            {
                // Eğer zaten bir BadRequest varsa ve ModelState hataları içeriyorsa, formatla
                if (badRequestResult.Value is Microsoft.AspNetCore.Mvc.SerializableError serializableError)
                {
                    var errors = FormatValidationErrors(context.ModelState);
                    
                    executedContext.Result = new BadRequestObjectResult(new ValidationErrorResponse
                    {
                        Success = false,
                        Message = "Validasyon hatası: Lütfen gönderilen verileri kontrol ediniz.",
                        Errors = errors,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
        }

        /// <summary>
        /// ModelState hatalarını standart formata çevirir
        /// </summary>
        private List<ValidationError> FormatValidationErrors(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            var errors = new List<ValidationError>();

            foreach (var key in modelState.Keys)
            {
                var state = modelState[key];
                if (state != null && state.Errors.Count > 0)
                {
                    var fieldName = key;
                    var errorMessages = state.Errors.Select(e => e.ErrorMessage).ToList();

                    // Eğer hata mesajı boşsa, genel bir mesaj ekle
                    if (errorMessages.All(string.IsNullOrWhiteSpace))
                    {
                        errorMessages = new List<string> { $"{fieldName} alanı geçersiz." };
                    }

                    errors.Add(new ValidationError
                    {
                        Field = fieldName,
                        Messages = errorMessages
                    });
                }
            }

            return errors;
        }
    }

    /// <summary>
    /// Validasyon hata yanıt modeli
    /// </summary>
    public class ValidationErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Tek bir alan için validasyon hatası
    /// </summary>
    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public List<string> Messages { get; set; } = new List<string>();
    }
}

