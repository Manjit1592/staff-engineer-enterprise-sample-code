namespace Enterprise.RagService.Application.Models;

public class RagRelevanceResult
{
    public bool IsRelevant { get; set; }

    public double Score { get; set; }

    public double MinimumRequiredScore { get; set; }
}