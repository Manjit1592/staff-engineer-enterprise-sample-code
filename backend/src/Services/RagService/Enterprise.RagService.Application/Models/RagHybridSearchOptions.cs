namespace Enterprise.RagService.Application.Models;

public class RagHybridSearchOptions
{
    public int RrfK { get; set; } = 60;

    public double VectorWeight { get; set; } = 1.0;

    public double KeywordWeight { get; set; } = 1.0;

    public bool ForceFailureForTesting { get; set; } = false;
}