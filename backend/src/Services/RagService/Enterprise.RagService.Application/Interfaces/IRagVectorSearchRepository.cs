using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagVectorSearchRepository
{
    Task<IReadOnlyList<RagSearchResult>> SearchAsync(
        float[] queryEmbedding,
        string tenantId,
        RagSearchFilter? filter = null,
        int topK = 5,
        CancellationToken cancellationToken = default);
}