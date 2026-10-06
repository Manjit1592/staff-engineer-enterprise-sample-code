using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagRelevanceService
{
    RagRelevanceResult Evaluate(
        double score);
}