using Enterprise.RagService.Application.Identity;
using Enterprise.RagService.Application.Security;

namespace Enterprise.RagService.Infrastructure.Security;

public class DevelopmentAuthorizationService
    : IAuthorizationService
{
    public AuthorizationResult Authorize(
        IUserIdentity identity,
        string requiredRole)
    {
        if (identity is null ||
            !identity.IsAuthenticated)
        {
            return AuthorizationResult.Deny(
                "UNAUTHENTICATED",
                "The user is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(requiredRole))
        {
            return AuthorizationResult.Deny(
                "INVALID_AUTHORIZATION_POLICY",
                "Required role is not configured.");
        }

        var hasRole =
            identity.Roles.Any(
                role =>
                    string.Equals(
                        role,
                        requiredRole,
                        StringComparison.OrdinalIgnoreCase));

        if (!hasRole)
        {
            return AuthorizationResult.Deny(
                "FORBIDDEN",
                $"User does not have the required role '{requiredRole}'.");
        }

        return AuthorizationResult.Allow();
    }

    public AuthorizationResult AuthorizeTenant(
        IUserIdentity identity,
        string tenantId)
    {
        if (identity is null ||
            !identity.IsAuthenticated)
        {
            return AuthorizationResult.Deny(
                "UNAUTHENTICATED",
                "The user is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return AuthorizationResult.Deny(
                "INVALID_TENANT",
                "TenantId is required.");
        }

        if (!string.Equals(
                identity.TenantId,
                tenantId,
                StringComparison.OrdinalIgnoreCase))
        {
            return AuthorizationResult.Deny(
                "TENANT_ACCESS_DENIED",
                "The authenticated user cannot access the requested tenant.");
        }

        return AuthorizationResult.Allow();
    }
}