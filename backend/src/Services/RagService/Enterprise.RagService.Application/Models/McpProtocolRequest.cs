namespace Enterprise.RagService.Application.Models;

public class McpProtocolRequest
{
    public string ToolName { get; set; } = string.Empty;

    public string Query { get; set; } = string.Empty;

    public IReadOnlyDictionary<string, object?> Arguments { get; set; }
        = new Dictionary<string, object?>();
}