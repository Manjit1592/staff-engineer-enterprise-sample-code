using System.Security.Claims;
using Enterprise.RagService.Application.Identity;

namespace Enterprise.RagService.Infrastructure.Security;

public class DevelopmentUserIdentity : IUserIdentity
{
    public DevelopmentUserIdentity(
        ClaimsPrincipal principal)
    {
        IsAuthenticated =
            principal.Identity?.IsAuthenticated == true;

        UserId =
            principal.FindFirst(
                ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value
            ?? string.Empty;

        TenantId =
            principal.FindFirst("tenant_id")?.Value
            ?? string.Empty;

        Roles =
            principal.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    public bool IsAuthenticated { get; }

    public string UserId { get; }

    public string TenantId { get; }

    public IReadOnlyList<string> Roles { get; }
}