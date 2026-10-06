namespace Enterprise.RagService.Application.Identity;

public interface IMcpUserContext
{
    string TenantId { get; }

    string UserId { get; }

    string Role { get; }

    bool IsAuthenticated { get; }
}