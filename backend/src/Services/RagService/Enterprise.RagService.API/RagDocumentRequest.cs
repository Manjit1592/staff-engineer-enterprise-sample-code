namespace Enterprise.RagService.API;

public class RagDocumentRequest
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;
}