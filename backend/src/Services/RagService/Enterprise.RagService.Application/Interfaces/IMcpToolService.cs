
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IMcpToolService
{
    Task<McpToolResponse> ExecuteAsync(
        McpToolRequest request,
        CancellationToken cancellationToken = default);
}