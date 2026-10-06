using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class RagAnswerService : IRagAnswerService
{
    private readonly IRagRetrievalWithFallbackService _retrievalService;
    private readonly IRagReranker _reranker;
    private readonly IRagRelevanceService _relevanceService;
    private readonly IRagContextBuilder _contextBuilder;
    private readonly IRagPromptBuilder _promptBuilder;
    private readonly ILlmService _llmService;
    private readonly IRagRetrievalGuard _retrievalGuard;

    public RagAnswerService(
    IRagRetrievalWithFallbackService retrievalService,
    IRagReranker reranker,
    IRagRelevanceService relevanceService,
    IRagContextBuilder contextBuilder,
    IRagPromptBuilder promptBuilder,
    ILlmService llmService,
    IRagRetrievalGuard retrievalGuard)
{
    _retrievalService = retrievalService;
    _reranker = reranker;
    _relevanceService = relevanceService;
    _contextBuilder = contextBuilder;
    _promptBuilder = promptBuilder;
    _llmService = llmService;
    _retrievalGuard = retrievalGuard;
}

    public async Task<RagAnswer> AskAsync(
        string query,
        string tenantId,
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

        var candidateCount = Math.Max(topK * 2, 10);

        var candidates =
            await _retrievalService.SearchAsync(
                query,
                tenantId,
                topK: candidateCount,
                cancellationToken: cancellationToken);

        if (candidates.Count == 0)
        {
            return CreateUnavailableAnswer();
        }

        var rerankedResults =
            await _reranker.RerankAsync(
                query,
                candidates,
                topK,
                cancellationToken);

        if (rerankedResults.Count == 0)
        {
            return CreateUnavailableAnswer();
        }

        var relevantResults = rerankedResults
    .Where(result =>
        _relevanceService
            .Evaluate(result.RerankScore)
            .IsRelevant)
    .Where(result =>
        _retrievalGuard
            .Evaluate(query, result.Result)
            .IsAllowed)
    .ToList();

        if (relevantResults.Count == 0)
        {
            return CreateUnavailableAnswer();
        }

        var searchResults = relevantResults
            .Select(result => result.Result)
            .ToList();

        var context =
            _contextBuilder.Build(searchResults);

        if (string.IsNullOrWhiteSpace(context.Content))
        {
            return CreateUnavailableAnswer();
        }

        var prompt =
            _promptBuilder.Build(
                query,
                context);

        var answer =
            await _llmService.GenerateAsync(
                prompt.SystemPrompt,
                prompt.UserPrompt,
                cancellationToken);

        var citations = searchResults
            .Select(result => new RagCitation
            {
                ChunkId = result.ChunkId,
                DocumentId = result.DocumentId,
                Source = result.Source,
                TenantId = result.TenantId,
                ChunkIndex = result.ChunkIndex
            })
            .ToList();

        return new RagAnswer
        {
            Answer = answer,
            Sources = searchResults,
            Citations = citations
        };
    }

    private static RagAnswer CreateUnavailableAnswer()
    {
        return new RagAnswer
        {
            Answer =
                "I could not find relevant information to answer this question.",

            Sources = Array.Empty<RagSearchResult>(),

            Citations = Array.Empty<RagCitation>()
        };
    }
}