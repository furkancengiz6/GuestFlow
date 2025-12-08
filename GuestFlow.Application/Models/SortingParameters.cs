namespace GuestFlow.Application.Models
{
    /// <summary>
    /// Sıralama parametreleri
    /// </summary>
    public class SortingParameters
    {
        /// <summary>
        /// Sıralama yapılacak alan adı
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sıralama yönü (asc, desc)
        /// </summary>
        public string? SortOrder { get; set; } = "asc";

        /// <summary>
        /// Sıralama yönü enum değeri
        /// </summary>
        public SortDirection Direction => 
            string.IsNullOrWhiteSpace(SortOrder) || SortOrder.ToLower() == "asc" 
                ? SortDirection.Ascending 
                : SortDirection.Descending;
    }

    /// <summary>
    /// Sıralama yönü
    /// </summary>
    public enum SortDirection
    {
        Ascending,
        Descending
    }
}

