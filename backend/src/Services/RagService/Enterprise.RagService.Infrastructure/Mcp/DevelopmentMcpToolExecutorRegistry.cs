using Enterprise.RagService.Application.Interfaces;

namespace Enterprise.RagService.Infrastructure.Mcp;

public class DevelopmentMcpToolExecutorRegistry
{
    private readonly IReadOnlyDictionary<
        string,
        IMcpToolExecutor> _executors;

    public DevelopmentMcpToolExecutorRegistry(
        IEnumerable<IMcpToolExecutor> executors)
    {
        _executors =
            executors.ToDictionary(
                executor => executor.ToolName,
                StringComparer.OrdinalIgnoreCase);
    }

    public IMcpToolExecutor? GetExecutor(
        string toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return null;
        }

        return _executors.TryGetValue(
            toolName,
            out var executor)
                ? executor
                : null;
    }
}