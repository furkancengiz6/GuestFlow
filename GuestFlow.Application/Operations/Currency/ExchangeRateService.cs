using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Currency
{
    /// <summary>
    /// Basit exchange rate servisi.
    /// Kaynak: appsettings: Accounting:ExchangeRates:{FROM}->{TO}
    /// Fallback: 1.0 (uyarı loglar).
    /// </summary>
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(IConfiguration configuration, ILogger<ExchangeRateService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
                return Task.FromResult(1.0m);

            if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(1.0m);

            var key = $"Accounting:ExchangeRates:{fromCurrency.ToUpperInvariant()}:{toCurrency.ToUpperInvariant()}";
            var raw = _configuration[key];
            if (!string.IsNullOrWhiteSpace(raw) &&
                decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) &&
                parsed > 0)
            {
                return Task.FromResult(parsed);
            }

            _logger.LogWarning("Exchange rate not found for {From}->{To}. Falling back to 1.0", fromCurrency, toCurrency);
            return Task.FromResult(1.0m);
        }

        public async Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            var rate = await GetExchangeRateAsync(fromCurrency, toCurrency);
            return amount * rate;
        }
    }
}
