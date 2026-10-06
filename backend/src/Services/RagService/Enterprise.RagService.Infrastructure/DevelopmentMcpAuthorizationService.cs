using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure;

public class DevelopmentMcpAuthorizationService
    : IMcpAuthorizationService
{
    private readonly IMcpToolPermissionService _permissionService;

    public DevelopmentMcpAuthorizationService(
        IMcpToolPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public McpAuthorizationResult Authorize(
        string tenantId,
        string role,
        McpToolDefinition tool)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return new McpAuthorizationResult
            {
                IsAllowed = false,
                Reason = "TenantId is required."
            };
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return new McpAuthorizationResult
            {
                IsAllowed = false,
                Reason = "Role is required."
            };
        }

        if (tool is null ||
            string.IsNullOrWhiteSpace(tool.Name))
        {
            return new McpAuthorizationResult
            {
                IsAllowed = false,
                Reason = "Tool is not available."
            };
        }

        if (!tenantId.StartsWith(
                "tenant-",
                StringComparison.OrdinalIgnoreCase))
        {
            return new McpAuthorizationResult
            {
                IsAllowed = false,
                Reason =
                    "Tenant is not authorized for MCP execution."
            };
        }

        var hasPermission =
            _permissionService.HasPermission(
                tenantId,
                role,
                tool);

        if (!hasPermission)
        {
            return new McpAuthorizationResult
            {
                IsAllowed = false,
                Reason =
                    $"Role '{role}' is not authorized " +
                    $"to execute tool '{tool.Name}'."
            };
        }

        return new McpAuthorizationResult
        {
            IsAllowed = true,
            Reason =
                "Tenant and role are authorized for the MCP tool."
        };
    }
}