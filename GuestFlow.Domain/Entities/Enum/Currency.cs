using System.Collections.Generic;
using System.Linq;

namespace GuestFlow.Domain.Entities.Enum
{
    /// <summary>
    /// Desteklenen para birimleri
    /// </summary>
    public enum Currency
    {
        TRY = 1,  // Türk Lirası
        USD = 2,  // Amerikan Doları
        EUR = 3,  // Euro
        GBP = 4,  // İngiliz Sterlini
        RUB = 5   // Rus Rublesi
    }

    /// <summary>
    /// Para birimi yardımcı sınıfı
    /// </summary>
    public static class CurrencyHelper
    {
        /// <summary>
        /// Para birimi kodunu string'e çevirir
        /// </summary>
        public static string ToCode(Currency currency)
        {
            return currency switch
            {
                Currency.TRY => "TRY",
                Currency.USD => "USD",
                Currency.EUR => "EUR",
                Currency.GBP => "GBP",
                Currency.RUB => "RUB",
                _ => "TRY"
            };
        }

        /// <summary>
        /// String kodunu Currency enum'una çevirir
        /// </summary>
        public static Currency FromCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Currency.TRY;

            return code.ToUpper() switch
            {
                "TRY" => Currency.TRY,
                "USD" => Currency.USD,
                "EUR" => Currency.EUR,
                "GBP" => Currency.GBP,
                "RUB" => Currency.RUB,
                _ => Currency.TRY
            };
        }

        /// <summary>
        /// Para birimi sembolünü döndürür
        /// </summary>
        public static string GetSymbol(Currency currency)
        {
            return currency switch
            {
                Currency.TRY => "₺",
                Currency.USD => "$",
                Currency.EUR => "€",
                Currency.GBP => "£",
                Currency.RUB => "₽",
                _ => "₺"
            };
        }

        /// <summary>
        /// Geçerli bir para birimi kodu mu kontrol eder
        /// </summary>
        public static bool IsValidCurrencyCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var validCodes = new[] { "TRY", "USD", "EUR", "GBP", "RUB" };
            return validCodes.Contains(code.ToUpper());
        }

        /// <summary>
        /// Tüm desteklenen para birimlerini döndürür
        /// </summary>
        public static List<string> GetAllCurrencyCodes()
        {
            return new List<string> { "TRY", "USD", "EUR", "GBP", "RUB" };
        }
    }
}

