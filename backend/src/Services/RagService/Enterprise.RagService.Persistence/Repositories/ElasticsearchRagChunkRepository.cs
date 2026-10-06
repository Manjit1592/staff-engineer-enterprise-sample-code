using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Domain.Entities;
using Enterprise.RagService.Persistence.Models;

namespace Enterprise.RagService.Persistence.Repositories;

public class ElasticsearchRagChunkRepository : IRagChunkRepository
{
    private const string IndexName = RagIndexNames.ActiveAlias;

    private readonly ElasticsearchClient _client;

    public ElasticsearchRagChunkRepository(
        ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task CreateIndexAsync(
        CancellationToken cancellationToken = default)
    {
        var existsResponse = await _client.Indices.ExistsAsync(
            IndexName,
            cancellationToken);

        if (existsResponse.Exists)
        {
            return;
        }

        var properties = new Properties
        {
            {
                "id",
                new KeywordProperty()
            },
            {
                "documentId",
                new KeywordProperty()
            },
            {
                "content",
                new TextProperty()
            },
            {
                "chunkIndex",
                new IntegerNumberProperty()
            },
            {
                "tenantId",
                new KeywordProperty()
            },
            {
                "source",
                new KeywordProperty()
            },
            {
                "embedding",
                new DenseVectorProperty
                {
                    Dims = 768,
                    Index = true,
                    Similarity = "cosine"
                }
            }
        };

        var response = await _client.Indices.CreateAsync(
            IndexName,
            descriptor => descriptor
                .Mappings(mappings => mappings
                    .Properties(properties)),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to create Elasticsearch index '{IndexName}'.");
        }
    }

    public async Task AddAsync(
        RagDocumentChunk chunk,
        CancellationToken cancellationToken = default)
    {
        var indexDocument = new RagChunkIndex
        {
            Id = chunk.Id,
            DocumentId = chunk.DocumentId,
            Content = chunk.Content,
            ChunkIndex = chunk.ChunkIndex,
            TenantId = chunk.TenantId,
            Source = chunk.Source,
            Embedding = chunk.Embedding
        };

        var response = await _client.IndexAsync(
            indexDocument,
            IndexName,
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to index RAG chunk '{chunk.Id}'.");
        }
    }
}