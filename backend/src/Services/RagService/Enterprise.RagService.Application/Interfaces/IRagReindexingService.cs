namespace Enterprise.RagService.Application.Interfaces;

public interface IRagReindexingService
{
    Task<int> ReindexAsync(
        CancellationToken cancellationToken = default);
}