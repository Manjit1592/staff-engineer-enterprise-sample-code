using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagRetrievalEvaluator
{
    Task<RagEvaluationSummary> EvaluateAsync(
        IReadOnlyList<RagEvaluationCase> evaluationCases,
        int topK = 5,
        CancellationToken cancellationToken = default);
}