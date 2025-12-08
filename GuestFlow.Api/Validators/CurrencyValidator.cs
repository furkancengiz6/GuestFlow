using FluentValidation;

namespace GuestFlow.Api.Validators
{
    /// <summary>
    /// Para birimi validasyonu için yardımcı sınıf
    /// </summary>
    public static class CurrencyValidator
    {
        private static readonly string[] ValidCurrencyCodes = { "TRY", "USD", "EUR", "GBP", "RUB" };

        /// <summary>
        /// Para birimi kodu validasyonu için FluentValidation kuralı
        /// </summary>
        public static IRuleBuilderOptions<T, string?> ValidateCurrencyCode<T>(IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(BeValidCurrencyCode)
                .WithMessage("Geçerli bir para birimi kodu giriniz (TRY, USD, EUR, GBP, RUB).")
                .When(x => !string.IsNullOrEmpty(x as string));
        }

        private static bool BeValidCurrencyCode(string? currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return true; // Opsiyonel alan, boş olabilir

            return ValidCurrencyCodes.Contains(currencyCode.ToUpper());
        }
    }
}

