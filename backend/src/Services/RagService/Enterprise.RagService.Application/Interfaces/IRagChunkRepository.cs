using Enterprise.RagService.Domain.Entities;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagChunkRepository
{
    Task CreateIndexAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RagDocumentChunk chunk,
        CancellationToken cancellationToken = default);
}