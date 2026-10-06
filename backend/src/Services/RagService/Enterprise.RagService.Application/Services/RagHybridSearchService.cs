using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class RagHybridSearchService : IRagHybridSearchService
{
    private readonly IRagRetrievalService _vectorSearchService;
    private readonly RagKeywordSearchService _keywordSearchService;
    private readonly RagHybridSearchOptions _options;

    public RagHybridSearchService(
        IRagRetrievalService vectorSearchService,
        RagKeywordSearchService keywordSearchService,
        RagHybridSearchOptions options)
    {
        _vectorSearchService = vectorSearchService;
        _keywordSearchService = keywordSearchService;
        _options = options;
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

if (_options.ForceFailureForTesting)
{
    throw new InvalidOperationException(
        "Forced hybrid retrieval failure for testing.");
}

        var retrievalTopK = Math.Max(topK * 2, 10);

        var vectorTask = _vectorSearchService.SearchAsync(
            query,
            tenantId,
            filter,
            retrievalTopK,
            cancellationToken);

        var keywordTask = _keywordSearchService.SearchAsync(
            query,
            tenantId,
            filter,
            retrievalTopK,
            cancellationToken);

        await Task.WhenAll(
            vectorTask,
            keywordTask);

        var vectorResults = await vectorTask;
        var keywordResults = await keywordTask;

        var mergedResults =
            new Dictionary<Guid, RagHybridSearchResult>();

        AddResults(
            mergedResults,
            vectorResults,
            isVectorResult: true);

        AddResults(
            mergedResults,
            keywordResults,
            isVectorResult: false);

        return mergedResults.Values
            .OrderByDescending(result => result.HybridScore)
            .Take(topK)
            .Select(result =>
            {
                result.Result.Score = result.HybridScore;
                return result.Result;
            })
            .ToList();
    }

    private void AddResults(
        Dictionary<Guid, RagHybridSearchResult> mergedResults,
        IReadOnlyList<RagSearchResult> results,
        bool isVectorResult)
    {
        for (var index = 0; index < results.Count; index++)
        {
            var result = results[index];

            if (!mergedResults.TryGetValue(
                    result.ChunkId,
                    out var hybridResult))
            {
                hybridResult = new RagHybridSearchResult
                {
                    Result = result
                };

                mergedResults[result.ChunkId] = hybridResult;
            }

            var rank = index + 1;

            var rrfScore =
                1.0 /
                (_options.RrfK + rank);

            if (isVectorResult)
            {
                hybridResult.VectorScore = result.Score;

                hybridResult.HybridScore +=
                    _options.VectorWeight * rrfScore;
            }
            else
            {
                hybridResult.KeywordScore = result.Score;

                hybridResult.HybridScore +=
                    _options.KeywordWeight * rrfScore;
            }
        }
    }
}