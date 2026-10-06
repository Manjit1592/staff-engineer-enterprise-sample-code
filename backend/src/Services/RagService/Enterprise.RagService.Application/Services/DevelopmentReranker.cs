using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class DevelopmentReranker : IRagReranker
{
    public Task<IReadOnlyList<RagRerankResult>> RerankAsync(
        string query,
        IReadOnlyList<RagSearchResult> candidates,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException(
                "Query cannot be empty.",
                nameof(query));
        }

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topK));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var queryTerms = query
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Select(term => term.ToLowerInvariant())
            .Distinct()
            .ToArray();

        var results = candidates
            .Select(candidate =>
            {
                var content =
                    candidate.Content.ToLowerInvariant();

                var matchedTerms = queryTerms.Count(
                    term => content.Contains(term));

                var score = queryTerms.Length == 0
                    ? 0
                    : (double)matchedTerms / queryTerms.Length;

                return new RagRerankResult
                {
                    Result = candidate,
                    RerankScore = score
                };
            })
            .OrderByDescending(result => result.RerankScore)
            .ThenByDescending(result => result.Result.Score)
            .Take(topK)
            .ToList();

        return Task.FromResult<
            IReadOnlyList<RagRerankResult>>(results);
    }
}