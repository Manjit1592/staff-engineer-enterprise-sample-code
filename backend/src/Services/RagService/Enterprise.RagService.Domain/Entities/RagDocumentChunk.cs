namespace Enterprise.RagService.Domain.Entities;

public class RagDocumentChunk
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public string TenantId { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public float[] Embedding { get; set; } = [];
}