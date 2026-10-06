using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class RagRetrievalService : IRagRetrievalService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IRagVectorSearchRepository _vectorSearchRepository;
    private readonly double _minimumScore;

    public RagRetrievalService(
        IEmbeddingService embeddingService,
        IRagVectorSearchRepository vectorSearchRepository,
        RagRetrievalOptions options)
    {
        _embeddingService = embeddingService;
        _vectorSearchRepository = vectorSearchRepository;
        _minimumScore = options.MinimumScore;
    }

    public async Task<IReadOnlyList<RagSearchResult>> SearchAsync(
        string query,
        string tenantId,
        RagSearchFilter? filter = null,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException(
                "Query cannot be empty.",
                nameof(query));
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException(
                "TenantId cannot be empty.",
                nameof(tenantId));
        }

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topK));
        }

        var queryEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(
                query,
                cancellationToken);

        var results =
            await _vectorSearchRepository.SearchAsync(
                queryEmbedding,
                tenantId,
                filter,
                topK,
                cancellationToken);

        return results
            .Where(result => result.Score >= _minimumScore)
            .ToList();
    }
}