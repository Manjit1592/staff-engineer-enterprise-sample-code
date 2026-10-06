namespace Enterprise.RagService.Application.Models;

public class AiOrchestratorResponse
{
    public string Query { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public IReadOnlyList<RagSearchResult> Sources { get; set; } =
        Array.Empty<RagSearchResult>();

    public IReadOnlyList<RagCitation> Citations { get; set; } =
        Array.Empty<RagCitation>();
}