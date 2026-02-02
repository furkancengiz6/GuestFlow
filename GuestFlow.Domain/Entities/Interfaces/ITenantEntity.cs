namespace GuestFlow.Domain.Entities.Interfaces
{
    /// <summary>
    /// Kiracı (Tenant) bazlı veri izolasyonu için arayüz.
    /// Bu arayüzü uygulayan tüm sınıflar bir TenantId'ye sahip olur ve otomatik filtrelenir.
    /// </summary>
    public interface ITenantEntity
    {
        int TenantId { get; set; }
    }
}
