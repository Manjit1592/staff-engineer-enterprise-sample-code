namespace Enterprise.RagService.Application.Models;

public class McpToolSelection
{
    public string ToolName { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public IReadOnlyDictionary<string, object?> Arguments { get; set; }
        = new Dictionary<string, object?>();
}