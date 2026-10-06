using System.Text.RegularExpressions;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class RagRetrievalGuard : IRagRetrievalGuard
{
    private static readonly string[] SuspiciousPatterns =
    [
        "ignore previous instructions",
        "ignore all previous instructions",
        "ignore the previous instructions",
        "reveal the system prompt",
        "reveal system prompt",
        "show the system prompt",
        "show system prompt",
        "developer message",
        "system message",
        "system instruction",
        "override instructions",
        "disregard previous instructions",
        "forget previous instructions",
        "you are now",
        "act as system"
    ];

    public RagRetrievalGuardResult Evaluate(
        string query,
        RagSearchResult result)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException(
                "Query cannot be empty.",
                nameof(query));
        }

        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (string.IsNullOrWhiteSpace(result.Content))
        {
            return new RagRetrievalGuardResult
            {
                IsAllowed = false,
                IsSuspicious = false,
                Reason = "Retrieved content is empty."
            };
        }

        var normalizedContent =
            Regex.Replace(
                result.Content.ToLowerInvariant(),
                @"\s+",
                " ")
            .Trim();

        foreach (var pattern in SuspiciousPatterns)
        {
            if (normalizedContent.Contains(pattern))
            {
                return new RagRetrievalGuardResult
                {
                    IsAllowed = false,
                    IsSuspicious = true,
                    Reason =
                        "Retrieved content contains a prompt-injection pattern."
                };
            }
        }

        return new RagRetrievalGuardResult
        {
            IsAllowed = true,
            IsSuspicious = false,
            Reason = "Retrieved content passed the retrieval safety check."
        };
    }
}