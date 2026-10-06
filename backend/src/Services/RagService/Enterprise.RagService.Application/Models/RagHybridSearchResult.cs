using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Models;

public class RagHybridSearchResult
{
    public RagSearchResult Result { get; set; } = new();

    public double VectorScore { get; set; }

    public double KeywordScore { get; set; }

    public double HybridScore { get; set; }
}