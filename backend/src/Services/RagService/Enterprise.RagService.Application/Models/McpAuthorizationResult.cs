namespace Enterprise.RagService.Application.Models;

public class McpAuthorizationResult
{
    public bool IsAllowed { get; set; }

    public string Reason { get; set; } = string.Empty;
}