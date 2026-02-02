using GuestFlow.Persistence.MultiTenancy;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GuestFlow.Api.Filters
{
    /// <summary>
    /// Her istekte JWT Claim üzerinden TenantId'yi okur ve TenantProvider'a aktarır.
    /// </summary>
    public class TenantFilter : IAsyncActionFilter
    {
        private readonly ITenantProvider _tenantProvider;

        public TenantFilter(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // JWT içindeki TenantId claim'ini bul
            var tenantIdClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "TenantId");
            
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                // TenantProvider'a set et (Scoped olduğu için bu request boyunca geçerli kalır)
                _tenantProvider.SetTenantId(tenantId);
            }
            else
            {
                // Eğer claim yoksa (örn: henüz login olmamış veya dış servis), 
                // default olarak 0 veya sistem belirlediği bir değer kalır.
                // Bazı durumlarda burada Unauthorized(401) dönmek gerekebilir.
            }

            await next();
        }
    }
}
