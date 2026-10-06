namespace Enterprise.RagService.Application.Models;

public class McpToolDefinition
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IReadOnlyList<string> RequiredArguments { get; set; }
        = Array.Empty<string>();

    public McpToolSchema InputSchema { get; set; }
        = new();

    public McpToolSchema OutputSchema { get; set; }
        = new();

    public IReadOnlyList<McpToolPermission> Permissions { get; set; }
        = Array.Empty<McpToolPermission>();

    public McpToolExecutionPolicy ExecutionPolicy { get; set; }
        = new();

    public IReadOnlyList<AiIntent> SupportedIntents { get; set; }
        = Array.Empty<AiIntent>();
}