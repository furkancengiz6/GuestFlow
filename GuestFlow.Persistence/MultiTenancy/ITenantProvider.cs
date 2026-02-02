namespace GuestFlow.Persistence.MultiTenancy
{
    public interface ITenantProvider
    {
        int TenantId { get; }
        void SetTenantId(int tenantId);
    }
}
