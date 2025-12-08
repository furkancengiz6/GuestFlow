using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Localization;
using GuestFlow.Application.Types;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Tüm controller'lar için base sınıf
    /// Standart API yanıt formatını sağlar
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private ILocalizationService? _localizationService;

        /// <summary>
        /// Lokalizasyon servisi (lazy loading)
        /// </summary>
        protected ILocalizationService Localization
        {
            get
            {
                if (_localizationService == null)
                {
                    _localizationService = HttpContext.RequestServices.GetService<ILocalizationService>();
                }
                return _localizationService ?? throw new InvalidOperationException("LocalizationService not registered.");
            }
        }

        /// <summary>
        /// Lokalize edilmiş string döndürür
        /// </summary>
        protected string L(string key, params object[] args)
        {
            try
            {
                return Localization.GetString(key, args);
            }
            catch
            {
                return key; // Fallback to key if localization fails
            }
        }

        /// <summary>
        /// Başarılı yanıt döndürür
        /// </summary>
        protected IActionResult Success<T>(T? data = default, string? message = null)
        {
            message ??= L("Success");
            return Ok(ApiResponse<T>.SuccessResponse(data, message));
        }

        /// <summary>
        /// Başarılı yanıt döndürür (veri olmadan)
        /// </summary>
        protected IActionResult Success(string? message = null)
        {
            message ??= L("Success");
            return Ok(ApiResponse.SuccessResponse(message));
        }

        /// <summary>
        /// Hata yanıtı döndürür
        /// </summary>
        protected IActionResult Error(string? message = null, int statusCode = 400, object? errors = null)
        {
            message ??= L("Error");
            var response = ApiResponse<object>.ErrorResponse(message, statusCode, errors);
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Bulunamadı yanıtı döndürür
        /// </summary>
        protected IActionResult NotFound(string? message = null)
        {
            message ??= L("NotFound");
            return NotFound(ApiResponse<object>.NotFoundResponse(message));
        }

        /// <summary>
        /// Yetkisiz erişim yanıtı döndürür
        /// </summary>
        protected IActionResult Unauthorized(string? message = null)
        {
            message ??= L("Unauthorized");
            return Unauthorized(ApiResponse<object>.UnauthorizedResponse(message));
        }

        /// <summary>
        /// ServiceMessage'ı API yanıtına çevirir
        /// </summary>
        protected IActionResult FromServiceMessage<T>(ServiceMessage serviceMessage, T? data = default)
        {
            if (serviceMessage.IsSuccess)
            {
                return Success(data, serviceMessage.Message);
            }
            else
            {
                return Error(serviceMessage.Message, 400);
            }
        }

        /// <summary>
        /// ServiceMessage'ı API yanıtına çevirir (veri olmadan)
        /// </summary>
        protected IActionResult FromServiceMessage(ServiceMessage serviceMessage)
        {
            if (serviceMessage.IsSuccess)
            {
                return Success(serviceMessage.Message);
            }
            else
            {
                return Error(serviceMessage.Message, 400);
            }
        }

        /// <summary>
        /// ServiceMessage&lt;T&gt;'ı API yanıtına çevirir
        /// </summary>
        protected IActionResult FromServiceMessage<T>(ServiceMessage<T> serviceMessage)
        {
            if (serviceMessage.IsSuccess)
            {
                return Success(serviceMessage.Data, serviceMessage.Message);
            }
            else
            {
                return Error(serviceMessage.Message, 400);
            }
        }

        /// <summary>
        /// Sayfalanmış sonuç döndürür
        /// </summary>
        protected IActionResult PagedResult<T>(GuestFlow.Application.Models.PagedResult<T> pagedResult, string? message = null)
        {
            message ??= L("DataRetrieved");
            // Application katmanındaki PagedResult'ı Api katmanındaki PagedResult'a dönüştür
            var apiPagedResult = new Models.PagedResult<T>(pagedResult.Data, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
            return Ok(ApiResponse<Models.PagedResult<T>>.SuccessResponse(apiPagedResult, message));
        }
    }
}

