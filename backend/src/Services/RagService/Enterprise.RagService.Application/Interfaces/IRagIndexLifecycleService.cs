namespace Enterprise.RagService.Application.Interfaces;

public interface IRagIndexLifecycleService
{
    Task<int> CleanupOldIndexesAsync(
        CancellationToken cancellationToken = default);
}