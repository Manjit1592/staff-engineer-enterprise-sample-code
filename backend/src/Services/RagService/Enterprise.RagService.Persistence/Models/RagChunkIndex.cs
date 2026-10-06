namespace Enterprise.RagService.Persistence.Models;

public class RagChunkIndex
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public string TenantId { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public float[] Embedding { get; set; } = [];
}