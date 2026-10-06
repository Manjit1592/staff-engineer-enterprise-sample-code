namespace Enterprise.RagService.Application.Models;

public class RagRetrievalGuardResult
{
    public bool IsAllowed { get; set; }

    public bool IsSuspicious { get; set; }

    public string Reason { get; set; } = string.Empty;
}