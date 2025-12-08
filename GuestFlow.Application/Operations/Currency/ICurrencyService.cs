using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Currency
{
    /// <summary>
    /// Para birimi yönetimi servisi
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Varsayılan para birimini döndürür
        /// </summary>
        string GetDefaultCurrency();

        /// <summary>
        /// Para birimi kodunun geçerli olup olmadığını kontrol eder
        /// </summary>
        bool IsValidCurrency(string currencyCode);

        /// <summary>
        /// Para birimi sembolünü döndürür
        /// </summary>
        string GetCurrencySymbol(string currencyCode);

        /// <summary>
        /// Tüm desteklenen para birimlerini döndürür
        /// </summary>
        List<CurrencyInfo> GetSupportedCurrencies();
    }

    /// <summary>
    /// Para birimi bilgisi
    /// </summary>
    public class CurrencyInfo
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
    }
}

