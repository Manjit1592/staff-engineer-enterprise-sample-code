namespace Enterprise.RagService.Application.Models;

public class RagEvaluationResult
{
    public string Name { get; set; } = string.Empty;

    public string Query { get; set; } = string.Empty;

    public int TopK { get; set; }

    public IReadOnlyList<Guid> RetrievedChunkIds { get; set; }
        = Array.Empty<Guid>();

    public int RelevantRetrievedCount { get; set; }

    public int ExpectedRelevantCount { get; set; }

    public double RecallAtK { get; set; }

    public double PrecisionAtK { get; set; }

    public double ReciprocalRank { get; set; }

    public int FirstRelevantRank { get; set; }

    public bool TenantIsolationPassed { get; set; } = true;

    public IReadOnlyList<Guid> ForbiddenChunksRetrieved { get; set; }
        = Array.Empty<Guid>();
}