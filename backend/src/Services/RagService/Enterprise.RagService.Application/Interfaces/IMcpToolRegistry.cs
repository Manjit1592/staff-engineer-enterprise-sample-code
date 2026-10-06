using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IMcpToolRegistry
{
    IReadOnlyList<McpToolDefinition> GetTools();

    McpToolDefinition? GetTool(
        string toolName);

    McpToolDefinition? GetToolForIntent(
        AiIntent intent);
}