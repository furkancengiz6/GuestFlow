using GuestFlow.Application.Operations.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class LocalizationController : BaseController
    {
        private readonly ILocalizationService _localizationService;

        public LocalizationController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Desteklenen dilleri getirir
        /// </summary>
        [HttpGet("languages")]
        public IActionResult GetSupportedLanguages()
        {
            var languages = _localizationService.GetSupportedLanguages();
            return Success(languages, "Desteklenen diller başarıyla getirildi.");
        }

        /// <summary>
        /// Mevcut kültürü getirir
        /// </summary>
        [HttpGet("current-culture")]
        public IActionResult GetCurrentCulture()
        {
            var culture = _localizationService.GetCurrentCulture();
            return Success(new { Culture = culture }, "Mevcut kültür başarıyla getirildi.");
        }
    }
}

