using GuestFlow.Application.Operations.Currency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")] // Admin ve Staff erişebilir
    public class CurrencyController : BaseController
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        /// <summary>
        /// Varsayılan para birimini getirir
        /// </summary>
        /// <summary>
        /// Varsayılan para birimini getirir
        /// </summary>
        [HttpGet("default")]
        public IActionResult GetDefaultCurrency()
        {
            try
            {
                var currency = _currencyService.GetDefaultCurrency();
                return Success(new { Currency = currency }, "Varsayılan para birimi başarıyla getirildi.");
            }
            catch (System.Exception ex)
            {
                return Error("Varsayılan para birimi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tüm desteklenen para birimlerini getirir
        /// </summary>
        [HttpGet("supported")]
        public IActionResult GetSupportedCurrencies()
        {
            try
            {
                var currencies = _currencyService.GetSupportedCurrencies();
                return Success(currencies, "Desteklenen para birimleri başarıyla getirildi.");
            }
            catch (System.Exception ex)
            {
                return Error("Desteklenen para birimleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Para birimi kodunun geçerli olup olmadığını kontrol eder
        /// </summary>
        [HttpGet("validate/{currencyCode}")]
        public IActionResult ValidateCurrency(string currencyCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currencyCode))
                {
                    return Error("Para birimi kodu gereklidir.", 400);
                }

                var isValid = _currencyService.IsValidCurrency(currencyCode);
                return Success(new { IsValid = isValid, CurrencyCode = currencyCode }, 
                    isValid ? "Para birimi kodu geçerlidir." : "Para birimi kodu geçersizdir.");
            }
            catch (System.Exception ex)
            {
                return Error("Para birimi validasyonu yapılırken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Para birimi sembolünü getirir
        /// </summary>
        [HttpGet("symbol/{currencyCode}")]
        public IActionResult GetCurrencySymbol(string currencyCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currencyCode))
                {
                    return Error("Para birimi kodu gereklidir.", 400);
                }

                var symbol = _currencyService.GetCurrencySymbol(currencyCode);
                return Success(new { CurrencyCode = currencyCode, Symbol = symbol }, 
                    "Para birimi sembolü başarıyla getirildi.");
            }
            catch (System.Exception ex)
            {
                return Error("Para birimi sembolü getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}

