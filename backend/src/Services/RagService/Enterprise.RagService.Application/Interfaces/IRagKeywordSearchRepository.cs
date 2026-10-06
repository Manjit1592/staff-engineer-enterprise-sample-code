using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagKeywordSearchRepository
{
    Task<IReadOnlyList<RagSearchResult>> SearchAsync(
        string query,
        string tenantId,
        RagSearchFilter? filter = null,
        int topK = 5,
        CancellationToken cancellationToken = default);
}