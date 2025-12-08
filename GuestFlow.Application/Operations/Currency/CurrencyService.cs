using GuestFlow.Domain.Entities.Enum;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Currency
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CurrencyService> _logger;
        private readonly string _defaultCurrency;

        public CurrencyService(IConfiguration configuration, ILogger<CurrencyService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _defaultCurrency = _configuration["CurrencySettings:DefaultCurrency"] ?? "TRY";
        }

        public string GetDefaultCurrency()
        {
            return _defaultCurrency;
        }

        public bool IsValidCurrency(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return false;

            return CurrencyHelper.IsValidCurrencyCode(currencyCode);
        }

        public string GetCurrencySymbol(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                currencyCode = _defaultCurrency;

            var currency = CurrencyHelper.FromCode(currencyCode);
            return CurrencyHelper.GetSymbol(currency);
        }

        public List<CurrencyInfo> GetSupportedCurrencies()
        {
            return new List<CurrencyInfo>
            {
                new CurrencyInfo { Code = "TRY", Name = "Türk Lirası", Symbol = "₺" },
                new CurrencyInfo { Code = "USD", Name = "Amerikan Doları", Symbol = "$" },
                new CurrencyInfo { Code = "EUR", Name = "Euro", Symbol = "€" },
                new CurrencyInfo { Code = "GBP", Name = "İngiliz Sterlini", Symbol = "£" },
                new CurrencyInfo { Code = "RUB", Name = "Rus Rublesi", Symbol = "₽" }
            };
        }
    }
}

