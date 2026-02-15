using System.Collections;
using System.Reflection;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Application.Operations.Privacy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GuestFlow.Api.Filters
{
    public class PiiMaskingActionFilter : IAsyncResultFilter
    {
        private readonly IPIIManagementService _piiService;

        public PiiMaskingActionFilter(IPIIManagementService piiService)
        {
            _piiService = piiService;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                MaskPii(objectResult.Value);
            }

            await next();
        }

        private void MaskPii(object? obj)
        {
            if (obj == null) return;

            var type = obj.GetType();

            if (obj is IEnumerable enumerable && !(obj is string))
            {
                foreach (var item in enumerable)
                {
                    MaskPii(item);
                }
                return;
            }

            if (type.IsPrimitive || type == typeof(string) || type.IsEnum) return;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                var maskAttr = prop.GetCustomAttribute<MaskPiiAttribute>();
                if (maskAttr != null && prop.PropertyType == typeof(string))
                {
                    var value = (string?)prop.GetValue(obj);
                    if (!string.IsNullOrEmpty(value))
                    {
                        var maskedValue = ApplyMask(value, maskAttr.Type);
                        prop.SetValue(obj, maskedValue);
                    }
                }
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    var value = prop.GetValue(obj);
                    MaskPii(value);
                }
            }
        }

        private string ApplyMask(string value, PiiType type)
        {
            return type switch
            {
                PiiType.Email => _piiService.MaskEmail(value),
                PiiType.Phone => _piiService.MaskPhone(value),
                PiiType.Passport => _piiService.MaskPassportNumber(value),
                PiiType.IdentityNumber => _piiService.MaskIdentityNumber(value),
                PiiType.Address => _piiService.MaskAddress(value),
                PiiType.CreditCard => _piiService.MaskCreditCard(value),
                _ => value
            };
        }
    }
}
