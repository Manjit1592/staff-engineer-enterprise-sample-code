namespace Enterprise.RagService.Application.Models;

public class RagEvaluationSummary
{
    public int TotalCases { get; set; }

    public int PassedCases { get; set; }

    public int FailedCases { get; set; }

    public double AverageRecallAtK { get; set; }

    public double AveragePrecisionAtK { get; set; }

    public double MeanReciprocalRank { get; set; }

    public bool TenantIsolationPassed { get; set; }

    public IReadOnlyList<RagEvaluationResult> Results { get; set; }
        = Array.Empty<RagEvaluationResult>();
}