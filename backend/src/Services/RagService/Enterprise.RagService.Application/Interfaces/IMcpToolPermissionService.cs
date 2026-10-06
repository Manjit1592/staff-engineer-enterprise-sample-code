using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IMcpToolPermissionService
{
    bool HasPermission(
        string tenantId,
        string role,
        McpToolDefinition tool);
}