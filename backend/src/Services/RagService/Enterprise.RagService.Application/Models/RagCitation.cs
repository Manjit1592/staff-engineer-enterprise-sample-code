namespace Enterprise.RagService.Application.Models;

public class RagCitation
{
    public Guid ChunkId { get; set; }

    public Guid DocumentId { get; set; }

    public string Source { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }
}