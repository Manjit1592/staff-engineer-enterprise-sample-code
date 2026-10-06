using Enterprise.RagService.Domain.Entities;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagDocumentRepository
{
    Task AddAsync(
        RagDocument document,
        CancellationToken cancellationToken = default);

    Task<RagDocument?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}