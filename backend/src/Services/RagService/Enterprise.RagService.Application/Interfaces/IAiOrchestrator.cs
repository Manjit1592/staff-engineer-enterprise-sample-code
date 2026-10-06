using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IAiOrchestrator
{
    Task<AiOrchestratorResponse> ExecuteAsync(
        AiOrchestratorRequest request,
        CancellationToken cancellationToken = default);
}