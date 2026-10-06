namespace Enterprise.RagService.Application.Identity;

public class McpUserContext : IMcpUserContext
{
    public string TenantId { get; init; } = string.Empty;

    public string UserId { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public bool IsAuthenticated { get; init; }
}