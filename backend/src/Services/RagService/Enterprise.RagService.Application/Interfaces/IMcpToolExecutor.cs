using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IMcpToolExecutor
{
    string ToolName { get; }

    Task<McpToolResponse> ExecuteAsync(
        McpToolExecutionContext context,
        CancellationToken cancellationToken = default);
}