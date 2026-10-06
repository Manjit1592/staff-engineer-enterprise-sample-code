using Enterprise.RagService.Domain.Entities;

namespace Enterprise.RagService.Application.Interfaces;

public interface IDocumentChunkingService
{
    IReadOnlyList<RagDocumentChunk> ChunkDocument(
        RagDocument document,
        int chunkSize = 500,
        int overlap = 50);
}