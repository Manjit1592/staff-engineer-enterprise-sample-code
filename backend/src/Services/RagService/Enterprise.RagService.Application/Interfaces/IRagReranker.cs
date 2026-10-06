using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagReranker
{
    Task<IReadOnlyList<RagRerankResult>> RerankAsync(
        string query,
        IReadOnlyList<RagSearchResult> candidates,
        int topK = 5,
        CancellationToken cancellationToken = default);
}