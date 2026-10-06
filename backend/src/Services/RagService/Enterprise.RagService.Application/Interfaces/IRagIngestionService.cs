using Enterprise.RagService.Domain.Entities;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagIngestionService
{
    Task IngestAsync(
        RagDocument document,
        CancellationToken cancellationToken = default);
}