namespace Enterprise.RagService.Application.Models;

public class McpToolValidationResult
{
    public bool IsValid { get; set; }

    public string Error { get; set; } = string.Empty;
}