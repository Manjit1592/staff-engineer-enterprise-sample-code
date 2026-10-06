using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;
using Microsoft.Extensions.Logging;

namespace Enterprise.RagService.Application.Services;

public class RagRetrievalWithFallbackService
    : IRagRetrievalWithFallbackService
{
    private readonly IRagHybridSearchService _hybridSearchService;
    private readonly IRagRetrievalService _vectorSearchService;
    private readonly RagRetrievalFallbackOptions _options;
    private readonly ILogger<RagRetrievalWithFallbackService> _logger;

    public RagRetrievalWithFallbackService(
        IRagHybridSearchService hybridSearchService,
        IRagRetrievalService vectorSearchService,
        RagRetrievalFallbackOptions options,
        ILogger<RagRetrievalWithFallbackService> logger)
    {
        _hybridSearchService = hybridSearchService;
        _vectorSearchService = vectorSearchService;
        _options = options;
        _logger = logger;
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

        try
        {
            var hybridResults =
                await _hybridSearchService.SearchAsync(
                    query,
                    tenantId,
                    filter,
                    topK,
                    cancellationToken);

            if (hybridResults.Count >= _options.MinimumResults)
            {
                return hybridResults;
            }

            _logger.LogWarning(
                "Hybrid retrieval returned insufficient results. " +
                "TenantId: {TenantId}, ResultCount: {ResultCount}, " +
                "MinimumRequired: {MinimumRequired}. " +
                "Using vector fallback.",
                tenantId,
                hybridResults.Count,
                _options.MinimumResults);
        }
        catch (Exception exception) when (
            !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                exception,
                "Hybrid retrieval failed. " +
                "TenantId: {TenantId}. Using vector fallback.",
                tenantId);
        }

        return await _vectorSearchService.SearchAsync(
            query,
            tenantId,
            filter,
            topK,
            cancellationToken);
    }
}