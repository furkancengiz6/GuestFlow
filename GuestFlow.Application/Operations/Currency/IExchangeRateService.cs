using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Currency
{
    /// <summary>
    /// Basit kur servisi arayüzü (multi-currency muhasebe için)
    /// </summary>
    public interface IExchangeRateService
    {
        /// <summary>
        /// fromCurrency -> toCurrency kurunu döndürür.
        /// </summary>
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);

        /// <summary>
        /// Tutarı belirtilen kura çevirir.
        /// </summary>
        Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}
