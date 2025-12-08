using GuestFlow.Application.Operations.Currency;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Common
{
    /// <summary>
    /// Fiyat hesaplama servisi - Tekrarlanan fiyat hesaplama mantığını merkezileştirir
    /// </summary>
    public interface IPriceCalculationService
    {
        /// <summary>
        /// İndirimli fiyatı hesaplar
        /// </summary>
        decimal CalculateFinalPrice(decimal price, decimal? discountPercentage);

        /// <summary>
        /// Para birimini validasyon yapar ve varsayılan para birimini döndürür
        /// </summary>
        string ValidateAndGetCurrency(string? currency);
    }

    public class PriceCalculationService : IPriceCalculationService
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<PriceCalculationService> _logger;

        public PriceCalculationService(
            ICurrencyService currencyService,
            ILogger<PriceCalculationService> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        public decimal CalculateFinalPrice(decimal price, decimal? discountPercentage)
        {
            decimal finalPrice = price;
            if (discountPercentage.HasValue && discountPercentage.Value > 0)
            {
                finalPrice -= finalPrice * (discountPercentage.Value / 100);
            }
            return finalPrice;
        }

        public string ValidateAndGetCurrency(string? currency)
        {
            var defaultCurrency = _currencyService.GetDefaultCurrency();
            
            if (string.IsNullOrWhiteSpace(currency))
            {
                return defaultCurrency;
            }

            if (!_currencyService.IsValidCurrency(currency))
            {
                _logger.LogWarning($"Geçersiz para birimi kodu: {currency}, varsayılan para birimi kullanılıyor: {defaultCurrency}");
                return defaultCurrency;
            }

            return currency;
        }
    }
}

