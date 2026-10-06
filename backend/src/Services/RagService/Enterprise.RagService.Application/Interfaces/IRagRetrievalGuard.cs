using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagRetrievalGuard
{
    RagRetrievalGuardResult Evaluate(
        string query,
        RagSearchResult result);
}