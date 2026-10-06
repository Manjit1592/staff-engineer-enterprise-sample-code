namespace Enterprise.RagService.Application.Models;

public class McpToolPermission
{
    public string ToolName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool Allowed { get; set; }
}