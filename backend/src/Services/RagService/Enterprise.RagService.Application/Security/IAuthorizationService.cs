using Enterprise.RagService.Application.Identity;

namespace Enterprise.RagService.Application.Security;

public interface IAuthorizationService
{
    AuthorizationResult Authorize(
        IUserIdentity identity,
        string requiredRole);

    AuthorizationResult AuthorizeTenant(
        IUserIdentity identity,
        string tenantId);
}