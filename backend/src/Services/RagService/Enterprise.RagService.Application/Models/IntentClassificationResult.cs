namespace Enterprise.RagService.Application.Models;

public class IntentClassificationResult
{
    public AiIntent Intent { get; set; }

    public string Reason { get; set; } = string.Empty;
}