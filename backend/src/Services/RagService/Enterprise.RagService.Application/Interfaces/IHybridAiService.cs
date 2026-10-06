using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IHybridAiService
{
    Task<HybridAiResponse> ExecuteAsync(
        HybridAiRequest request,
        CancellationToken cancellationToken = default);
}