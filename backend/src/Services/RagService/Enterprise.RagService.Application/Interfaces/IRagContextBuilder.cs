using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagContextBuilder
{
    RagContext Build(
        IReadOnlyList<RagSearchResult> searchResults);
}