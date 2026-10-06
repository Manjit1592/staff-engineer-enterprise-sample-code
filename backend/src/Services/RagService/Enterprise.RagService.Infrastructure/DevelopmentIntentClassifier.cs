using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure;

public class DevelopmentIntentClassifier : IIntentClassifier
{
    public Task<IntentClassificationResult> ClassifyAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(
                new IntentClassificationResult
                {
                    Intent = AiIntent.Unknown,
                    Reason = "Query is empty."
                });
        }

        var normalizedQuery =
            query.Trim().ToLowerInvariant();

        var actionKeywords = new[]
        {
            "create",
            "delete",
            "update",
            "cancel",
            "place order",
            "submit",
            "approve",
            "execute"
        };

        var liveDataKeywords = new[]
        {
            "current",
            "latest",
            "live",
            "real-time",
            "realtime",
            "status",
            "available",
            "inventory",
            "stock",
            "quantity",
            "warehouse"
        };

        var hasActionKeyword =
            actionKeywords.Any(
                normalizedQuery.Contains);

        var hasLiveDataKeyword =
            liveDataKeywords.Any(
                normalizedQuery.Contains);

        if (hasActionKeyword && hasLiveDataKeyword)
        {
            return Task.FromResult(
                new IntentClassificationResult
                {
                    Intent = AiIntent.Hybrid,
                    Reason =
                        "Query contains both action and live-data indicators."
                });
        }

        if (hasActionKeyword)
        {
            return Task.FromResult(
                new IntentClassificationResult
                {
                    Intent = AiIntent.Action,
                    Reason =
                        "Query contains an action indicator."
                });
        }

        if (hasLiveDataKeyword)
        {
            return Task.FromResult(
                new IntentClassificationResult
                {
                    Intent = AiIntent.LiveData,
                    Reason =
                        "Query contains a live-data indicator."
                });
        }

        return Task.FromResult(
            new IntentClassificationResult
            {
                Intent = AiIntent.Knowledge,
                Reason =
                    "Query does not contain action or live-data indicators."
            });
    }
}