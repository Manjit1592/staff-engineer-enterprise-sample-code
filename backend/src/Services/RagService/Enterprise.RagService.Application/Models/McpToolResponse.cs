namespace Enterprise.RagService.Application.Models;

public class McpToolResponse
{
    public bool Success { get; set; }

    public string Result { get; set; } = string.Empty;

    public string ToolName { get; set; } = string.Empty;

    public object? Data { get; set; }
}