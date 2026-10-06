namespace Enterprise.RagService.Application.Models;

public class RagAnswer
{
    public string Answer { get; set; } = string.Empty;

    public IReadOnlyList<RagSearchResult> Sources { get; set; }
        = Array.Empty<RagSearchResult>();

    public IReadOnlyList<RagCitation> Citations { get; set; }
        = Array.Empty<RagCitation>();
}