namespace GuestFlow.Domain.Entities.Enum
{
    /// <summary>
    /// Paket tipleri
    /// </summary>
    public enum PackageType
    {
        /// <summary>
        /// Standart paket
        /// </summary>
        Standard = 1,

        /// <summary>
        /// Premium paket
        /// </summary>
        Premium = 2,

        /// <summary>
        /// VIP paket
        /// </summary>
        VIP = 3,

        /// <summary>
        /// Özel paket
        /// </summary>
        Custom = 4
    }

    /// <summary>
    /// Paket tipi yardımcı sınıfı
    /// </summary>
    public static class PackageTypeHelper
    {
        /// <summary>
        /// Tipi string'e çevirir
        /// </summary>
        public static string ToString(PackageType type)
        {
            return type switch
            {
                PackageType.Standard => "Standard",
                PackageType.Premium => "Premium",
                PackageType.VIP => "VIP",
                PackageType.Custom => "Custom",
                _ => "Custom"
            };
        }

        /// <summary>
        /// String'i PackageType enum'una çevirir
        /// </summary>
        public static PackageType FromString(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return PackageType.Custom;

            return type.ToLower() switch
            {
                "standard" or "standart" => PackageType.Standard,
                "premium" => PackageType.Premium,
                "vip" => PackageType.VIP,
                "custom" or "özel" => PackageType.Custom,
                _ => PackageType.Custom
            };
        }

        /// <summary>
        /// Tipin Türkçe karşılığını döndürür
        /// </summary>
        public static string GetTurkishName(PackageType type)
        {
            return type switch
            {
                PackageType.Standard => "Standart",
                PackageType.Premium => "Premium",
                PackageType.VIP => "VIP",
                PackageType.Custom => "Özel",
                _ => "Özel"
            };
        }
    }
}

