using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class RagRelevanceService : IRagRelevanceService
{
    private readonly RagRetrievalOptions _options;

    public RagRelevanceService(
        RagRetrievalOptions options)
    {
        _options = options;
    }

    public RagRelevanceResult Evaluate(
        double score)
    {
        var minimumScore =
            Math.Clamp(
                _options.MinimumScore,
                0.0,
                1.0);

        return new RagRelevanceResult
        {
            Score = score,
            MinimumRequiredScore = minimumScore,
            IsRelevant = score >= minimumScore
        };
    }
}