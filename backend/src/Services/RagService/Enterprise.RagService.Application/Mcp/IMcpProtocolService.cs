using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Mcp;

public interface IMcpProtocolService
{
    Task<McpProtocolResponse> InvokeAsync(
        McpProtocolRequest request,
        CancellationToken cancellationToken = default);
}