namespace GuestFlow.Domain.Entities.Enum
{
    /// <summary>
    /// Ödeme yöntemleri
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Kredi Kartı
        /// </summary>
        CreditCard = 1,

        /// <summary>
        /// Banka Havalesi/Eft
        /// </summary>
        BankTransfer = 2,

        /// <summary>
        /// Nakit
        /// </summary>
        Cash = 3,

        /// <summary>
        /// Diğer
        /// </summary>
        Other = 4
    }

    /// <summary>
    /// Ödeme yöntemi yardımcı sınıfı
    /// </summary>
    public static class PaymentMethodHelper
    {
        /// <summary>
        /// Ödeme yöntemini string'e çevirir
        /// </summary>
        public static string ToString(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.CreditCard => "CreditCard",
                PaymentMethod.BankTransfer => "BankTransfer",
                PaymentMethod.Cash => "Cash",
                PaymentMethod.Other => "Other",
                _ => "Other"
            };
        }

        /// <summary>
        /// String'i PaymentMethod enum'una çevirir
        /// </summary>
        public static PaymentMethod FromString(string method)
        {
            if (string.IsNullOrWhiteSpace(method))
                return PaymentMethod.Other;

            return method.ToLower() switch
            {
                "creditcard" or "credit_card" or "credit" => PaymentMethod.CreditCard,
                "banktransfer" or "bank_transfer" or "transfer" or "eft" or "havale" => PaymentMethod.BankTransfer,
                "cash" or "nakit" => PaymentMethod.Cash,
                "other" or "diğer" => PaymentMethod.Other,
                _ => PaymentMethod.Other
            };
        }

        /// <summary>
        /// Ödeme yönteminin Türkçe karşılığını döndürür
        /// </summary>
        public static string GetTurkishName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.CreditCard => "Kredi Kartı",
                PaymentMethod.BankTransfer => "Banka Havalesi",
                PaymentMethod.Cash => "Nakit",
                PaymentMethod.Other => "Diğer",
                _ => "Diğer"
            };
        }

        /// <summary>
        /// Geçerli bir ödeme yöntemi olup olmadığını kontrol eder
        /// </summary>
        public static bool IsValidMethod(string method)
        {
            if (string.IsNullOrWhiteSpace(method))
                return false;

            var validMethods = new[] { "creditcard", "credit_card", "credit", "banktransfer", "bank_transfer", "transfer", "eft", "havale", "cash", "nakit", "other", "diğer" };
            return validMethods.Contains(method.ToLower());
        }

        /// <summary>
        /// Tüm ödeme yöntemlerini listeler
        /// </summary>
        public static List<string> GetAllMethods()
        {
            return new List<string> { "CreditCard", "BankTransfer", "Cash", "Other" };
        }
    }
}

