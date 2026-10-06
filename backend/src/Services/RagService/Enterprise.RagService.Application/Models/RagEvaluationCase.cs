namespace Enterprise.RagService.Application.Models;

public class RagEvaluationCase
{
    public string Name { get; set; } = string.Empty;

    public string Query { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string? Source { get; set; }

    public IReadOnlyList<Guid> ExpectedChunkIds { get; set; }
        = Array.Empty<Guid>();

    public IReadOnlyList<Guid> ForbiddenChunkIds { get; set; }
        = Array.Empty<Guid>();
}