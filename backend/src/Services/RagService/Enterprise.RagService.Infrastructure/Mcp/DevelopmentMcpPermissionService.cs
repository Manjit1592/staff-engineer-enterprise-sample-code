using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure.Mcp;

public class DevelopmentMcpPermissionService
    : IMcpToolPermissionService
{
    public bool HasPermission(
        string tenantId,
        string role,
        McpToolDefinition tool)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(role) ||
            tool is null)
        {
            return false;
        }

        if (!tenantId.StartsWith(
                "tenant-",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var permission =
            tool.Permissions.FirstOrDefault(
                item =>
                    string.Equals(
                        item.ToolName,
                        tool.Name,
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    string.Equals(
                        item.Role,
                        role,
                        StringComparison.OrdinalIgnoreCase));

        return permission?.Allowed == true;
    }
}