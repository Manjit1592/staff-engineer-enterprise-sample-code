namespace Enterprise.RagService.Application.Models;

public class RagRerankResult
{
    public RagSearchResult Result { get; set; } = new();

    public double RerankScore { get; set; }
}