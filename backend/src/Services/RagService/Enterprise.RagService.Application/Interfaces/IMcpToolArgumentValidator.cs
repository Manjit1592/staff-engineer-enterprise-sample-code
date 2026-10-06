using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IMcpToolArgumentValidator
{
    McpToolValidationResult Validate(
        McpToolDefinition tool,
        IReadOnlyDictionary<string, object?> arguments);
}