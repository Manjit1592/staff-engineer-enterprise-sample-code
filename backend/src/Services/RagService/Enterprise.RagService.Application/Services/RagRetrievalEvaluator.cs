using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class RagRetrievalEvaluator : IRagRetrievalEvaluator
{
    private readonly IRagHybridSearchService _hybridSearchService;

    public RagRetrievalEvaluator(
        IRagHybridSearchService hybridSearchService)
    {
        _hybridSearchService = hybridSearchService;
    }

    public async Task<RagEvaluationSummary> EvaluateAsync(
        IReadOnlyList<RagEvaluationCase> evaluationCases,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (evaluationCases is null)
        {
            throw new ArgumentNullException(nameof(evaluationCases));
        }

        if (evaluationCases.Count == 0)
        {
            throw new ArgumentException(
                "At least one evaluation case is required.",
                nameof(evaluationCases));
        }

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topK));
        }

        var results = new List<RagEvaluationResult>();

        foreach (var evaluationCase in evaluationCases)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ValidateCase(evaluationCase);

            var filter = string.IsNullOrWhiteSpace(
                evaluationCase.Source)
                ? null
                : new RagSearchFilter
                {
                    Source = evaluationCase.Source
                };

            var retrieved =
                await _hybridSearchService.SearchAsync(
                    evaluationCase.Query,
                    evaluationCase.TenantId,
                    filter,
                    topK,
                    cancellationToken);

            var expectedIds =
                evaluationCase.ExpectedChunkIds.ToHashSet();

            var forbiddenIds =
                evaluationCase.ForbiddenChunkIds.ToHashSet();

            var retrievedIds =
                retrieved
                    .Select(result => result.ChunkId)
                    .ToList();

            var relevantRetrievedCount =
                retrievedIds.Count(expectedIds.Contains);

            var firstRelevantRank = 0;

            for (var index = 0;
                 index < retrievedIds.Count;
                 index++)
            {
                if (expectedIds.Contains(retrievedIds[index]))
                {
                    firstRelevantRank = index + 1;
                    break;
                }
            }

            var recall =
                expectedIds.Count == 0
                    ? 0
                    : (double)relevantRetrievedCount /
                      expectedIds.Count;

            var precision =
                topK == 0
                    ? 0
                    : (double)relevantRetrievedCount /
                      topK;

            var reciprocalRank =
                firstRelevantRank == 0
                    ? 0
                    : 1.0 / firstRelevantRank;

            var forbiddenRetrieved =
                retrievedIds
                    .Where(forbiddenIds.Contains)
                    .Distinct()
                    .ToList();

            var tenantIsolationPassed =
                forbiddenRetrieved.Count == 0;

            results.Add(
                new RagEvaluationResult
                {
                    Name = evaluationCase.Name,
                    Query = evaluationCase.Query,
                    TopK = topK,
                    RetrievedChunkIds = retrievedIds,
                    RelevantRetrievedCount =
                        relevantRetrievedCount,
                    ExpectedRelevantCount =
                        expectedIds.Count,
                    RecallAtK = recall,
                    PrecisionAtK = precision,
                    ReciprocalRank = reciprocalRank,
                    FirstRelevantRank = firstRelevantRank,
                    TenantIsolationPassed =
                        tenantIsolationPassed,
                    ForbiddenChunksRetrieved =
                        forbiddenRetrieved
                });
        }

        return BuildSummary(results);
    }

    private static void ValidateCase(
        RagEvaluationCase evaluationCase)
    {
        if (evaluationCase is null)
        {
            throw new ArgumentNullException(
                nameof(evaluationCase));
        }

        if (string.IsNullOrWhiteSpace(
                evaluationCase.Query))
        {
            throw new ArgumentException(
                "Evaluation query cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(
                evaluationCase.TenantId))
        {
            throw new ArgumentException(
                "Evaluation tenant cannot be empty.");
        }

        if (evaluationCase.ExpectedChunkIds.Count == 0 &&
            evaluationCase.ForbiddenChunkIds.Count == 0)
        {
            throw new ArgumentException(
                "At least one expected or forbidden chunk ID is required.");
        }
    }

    private static RagEvaluationSummary BuildSummary(
        IReadOnlyList<RagEvaluationResult> results)
    {
        var totalCases = results.Count;

        var passedCases =
            results.Count(
                result =>
                    result.RecallAtK > 0 ||
                    (
                        result.ExpectedRelevantCount == 0 &&
                        result.TenantIsolationPassed
                    ));

        var failedCases =
            totalCases - passedCases;

        return new RagEvaluationSummary
        {
            TotalCases = totalCases,
            PassedCases = passedCases,
            FailedCases = failedCases,

            AverageRecallAtK =
                results.Average(
                    result => result.RecallAtK),

            AveragePrecisionAtK =
                results.Average(
                    result => result.PrecisionAtK),

            MeanReciprocalRank =
                results.Average(
                    result => result.ReciprocalRank),

            TenantIsolationPassed =
                results.All(
                    result => result.TenantIsolationPassed),

            Results = results
        };
    }
}