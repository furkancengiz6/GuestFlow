using Microsoft.Extensions.Localization;
using System.Globalization;
using GuestFlow.Application.Resources;

namespace GuestFlow.Application.Operations.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public LocalizationService(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
        }

        public string GetString(string key, params object[] args)
        {
            var localizedString = _localizer[key];
            
            if (args != null && args.Length > 0)
            {
                return string.Format(localizedString.Value, args);
            }

            return localizedString.Value;
        }

        public string GetCurrentCulture()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        public List<SupportedLanguage> GetSupportedLanguages()
        {
            return new List<SupportedLanguage>
            {
                new SupportedLanguage
                {
                    Code = "tr-TR",
                    Name = "Turkish",
                    NativeName = "Türkçe"
                },
                new SupportedLanguage
                {
                    Code = "en-US",
                    Name = "English",
                    NativeName = "English"
                }
            };
        }
    }
}

