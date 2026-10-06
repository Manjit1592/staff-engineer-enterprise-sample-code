namespace Enterprise.RagService.Application.Models;

public class RagSearchResult
{
    public Guid ChunkId { get; set; }

    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public string TenantId { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public double Score { get; set; }
}