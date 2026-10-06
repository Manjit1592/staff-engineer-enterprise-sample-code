using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagAnswerService
{
    Task<RagAnswer> AskAsync(
        string query,
        string tenantId,
        int topK = 5,
        CancellationToken cancellationToken = default);
}