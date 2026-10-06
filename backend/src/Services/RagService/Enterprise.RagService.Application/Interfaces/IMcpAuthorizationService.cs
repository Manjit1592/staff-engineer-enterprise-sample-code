using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IMcpAuthorizationService
{
    McpAuthorizationResult Authorize(
        string tenantId,
        string role,
        McpToolDefinition tool);
}