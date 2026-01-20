using Microsoft.AspNetCore.Http;
using Konta.Shared.Data;
using System.Security.Claims;

namespace Konta.Shared.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                tenantContext.TenantId = tenantId;
            }
        }

        await _next(context);
    }
}
