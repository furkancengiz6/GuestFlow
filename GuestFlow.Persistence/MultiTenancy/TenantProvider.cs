namespace GuestFlow.Persistence.MultiTenancy
{
    public class TenantProvider : ITenantProvider
    {
        public int TenantId { get; private set; }

        public void SetTenantId(int tenantId)
        {
            TenantId = tenantId;
        }
    }
}
