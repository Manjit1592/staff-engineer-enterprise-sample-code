namespace Enterprise.RagService.Application.Models;

public class HybridAiResponse
{
    public string RagAnswer { get; set; } = string.Empty;

    public string McpResult { get; set; } = string.Empty;

    public IReadOnlyList<RagSearchResult> Sources { get; set; }
        = Array.Empty<RagSearchResult>();

    public IReadOnlyList<RagCitation> Citations { get; set; }
        = Array.Empty<RagCitation>();
}