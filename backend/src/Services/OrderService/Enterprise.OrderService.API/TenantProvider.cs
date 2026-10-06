using Enterprise.OrderService.Application.Interfaces;
using System.Security.Claims;

namespace Enterprise.OrderService.API;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var tenantId =
                _httpContextAccessor
                    .HttpContext?
                    .User?
                    .FindFirst("TenantId")
                    ?.Value;

            if (Guid.TryParse(
                tenantId,
                out var parsedTenantId))
            {
                return parsedTenantId;
            }

            return Guid.Empty;
        }
    }
}