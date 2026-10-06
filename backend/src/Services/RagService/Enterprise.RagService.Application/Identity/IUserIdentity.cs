
namespace Enterprise.RagService.Application.Identity;

public interface IUserIdentity
{
    bool IsAuthenticated { get; }

    string UserId { get; }

    string TenantId { get; }

    IReadOnlyList<string> Roles { get; }
}