using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class RagKeywordSearchService
{
    private readonly IRagKeywordSearchRepository _repository;

    public RagKeywordSearchService(
        IRagKeywordSearchRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RagSearchResult>> SearchAsync(
        string query,
        string tenantId,
        RagSearchFilter? filter = null,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        return await _repository.SearchAsync(
            query,
            tenantId,
            filter,
            topK,
            cancellationToken);
    }
}