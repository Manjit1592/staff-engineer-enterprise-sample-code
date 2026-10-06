namespace Enterprise.RagService.Application.Models;

public class McpProtocolResponse
{
    public bool Success { get; set; }

    public string ToolName { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public object? Data { get; set; }

    public string ErrorCode { get; set; } = string.Empty;
}