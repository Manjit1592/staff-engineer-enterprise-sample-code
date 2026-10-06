namespace Enterprise.RagService.API;

public class RagEvaluationRequest
{
    public int TopK { get; set; } = 5;

    public IReadOnlyList<RagEvaluationCaseRequest> Cases { get; set; }
        = Array.Empty<RagEvaluationCaseRequest>();
}

public class RagEvaluationCaseRequest
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