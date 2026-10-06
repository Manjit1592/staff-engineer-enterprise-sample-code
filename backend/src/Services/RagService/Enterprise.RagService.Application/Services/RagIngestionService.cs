using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Domain.Entities;

namespace Enterprise.RagService.Application.Services;

public class RagIngestionService : IRagIngestionService
{
    private readonly IRagDocumentRepository _documentRepository;
    private readonly IRagChunkRepository _chunkRepository;
    private readonly IDocumentChunkingService _chunkingService;
    private readonly IEmbeddingService _embeddingService;

    public RagIngestionService(
        IRagDocumentRepository documentRepository,
        IRagChunkRepository chunkRepository,
        IDocumentChunkingService chunkingService,
        IEmbeddingService embeddingService)
    {
        _documentRepository = documentRepository;
        _chunkRepository = chunkRepository;
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
    }

    public async Task IngestAsync(
        RagDocument document,
        CancellationToken cancellationToken = default)
    {
        await _documentRepository.AddAsync(
            document,
            cancellationToken);

        await _chunkRepository.CreateIndexAsync(
            cancellationToken);

        var chunks = _chunkingService.ChunkDocument(document);

        foreach (var chunk in chunks)
        {
            chunk.Embedding =
                await _embeddingService.GenerateEmbeddingAsync(
                    chunk.Content,
                    cancellationToken);

            await _chunkRepository.AddAsync(
                chunk,
                cancellationToken);
        }
    }
}