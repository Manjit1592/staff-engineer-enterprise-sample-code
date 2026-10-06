namespace Enterprise.RagService.Application.Models;

public class McpArgumentExtractionResult
{
    public bool Success { get; set; }

    public string Error { get; set; } = string.Empty;

    public IReadOnlyDictionary<string, object?> Arguments { get; set; }
        = new Dictionary<string, object?>();
}