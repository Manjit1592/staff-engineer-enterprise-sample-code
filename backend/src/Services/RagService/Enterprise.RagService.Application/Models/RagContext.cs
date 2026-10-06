namespace Enterprise.RagService.Application.Models;

public class RagContext
{
    public string Content { get; set; } = string.Empty;

    public IReadOnlyList<RagSearchResult> Sources { get; set; }
        = Array.Empty<RagSearchResult>();
}