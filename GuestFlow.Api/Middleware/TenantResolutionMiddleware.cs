using GuestFlow.Persistence.MultiTenancy;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace GuestFlow.Api.Middleware
{
    /// <summary>
    /// Middleware to resolve the current TenantId from the request context.
    /// Priority:
    /// 1. Authenticated User Claim ("TenantId")
    /// 2. Header ("X-Tenant-ID") - For system tools/webhooks
    /// 3. Query String ("tenantId") - For debugging
    /// 4. Default to 0 (or 1) if not found.
    /// </summary>
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            int tenantId = 0;
            bool found = false;

            // 1. Check Authenticated User Claim (Highest Trust)
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = context.User.Claims.FirstOrDefault(c => c.Type == "TenantId");
                if (tenantClaim != null && int.TryParse(tenantClaim.Value, out int id))
                {
                    tenantId = id;
                    found = true;
                }
            }

            // 2. Check Header (if not found in claims)
            if (!found && context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerValue))
            {
                if (int.TryParse(headerValue, out int id))
                {
                    tenantId = id;
                    found = true;
                }
            }

            // 3. Check Query String (if not found in others)
            if (!found && context.Request.Query.TryGetValue("tenantId", out var queryValue))
            {
                if (int.TryParse(queryValue, out int id))
                {
                    tenantId = id;
                    found = true;
                }
            }

            // Default fallback logic could go here (e.g., default to Main Tenant 1)
            if (!found)
            {
                 // Optional: Set default tenant if appropriate for your logic
                 // tenantId = 1; 
            }

            // Set the TenantId in the scoped provider
            // NOTE: TenantProvider is Scoped, so we resolve it from RequestServices
            if (tenantId > 0)
            {
                var tenantProvider = context.RequestServices.GetService<ITenantProvider>();
                if (tenantProvider != null)
                {
                    tenantProvider.SetTenantId(tenantId);
                }
            }

            await _next(context);
        }
    }
}
