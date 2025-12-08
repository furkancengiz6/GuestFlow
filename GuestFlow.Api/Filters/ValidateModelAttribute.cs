using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GuestFlow.Api.Filters
{
    /// <summary>
    /// Model validasyonu için Attribute
    /// Controller veya Action seviyesinde kullanılabilir
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<ValidateModelAttribute>)) as ILogger<ValidateModelAttribute>;
                
                var errors = FormatValidationErrors(context.ModelState);
                
                logger?.LogWarning($"Model validasyon hatası: {context.ActionDescriptor.DisplayName}. Hatalar: {string.Join(", ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Messages)}"))}");

                context.Result = new BadRequestObjectResult(new ValidationErrorResponse
                {
                    Success = false,
                    Message = "Validasyon hatası: Lütfen gönderilen verileri kontrol ediniz.",
                    Errors = errors,
                    Timestamp = DateTime.UtcNow
                });
            }

            base.OnActionExecuting(context);
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
}

