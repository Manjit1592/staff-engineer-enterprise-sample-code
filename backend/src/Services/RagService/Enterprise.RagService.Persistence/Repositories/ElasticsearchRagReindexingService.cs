using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Domain.Entities;

namespace Enterprise.RagService.Persistence.Repositories;

public class ElasticsearchRagReindexingService
    : IRagReindexingService
{
    private const string ActiveAlias =
        RagIndexNames.ActiveAlias;

    private const int EmbeddingDimensions = 768;

    private readonly ElasticsearchClient _client;
    private readonly IEmbeddingService _embeddingService;

    public ElasticsearchRagReindexingService(
        ElasticsearchClient client,
        IEmbeddingService embeddingService)
    {
        _client = client;
        _embeddingService = embeddingService;
    }

    public async Task<int> ReindexAsync(
        CancellationToken cancellationToken = default)
    {
        var targetIndexName =
            await GetNextIndexNameAsync(
                cancellationToken);

        await CreateTargetIndexAsync(
            targetIndexName,
            cancellationToken);

        var chunks =
            await ReadExistingChunksAsync(
                cancellationToken);

        var count = 0;

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            chunk.Embedding =
                await _embeddingService.GenerateEmbeddingAsync(
                    chunk.Content,
                    cancellationToken);

            await IndexChunkAsync(
                chunk,
                targetIndexName,
                cancellationToken);

            count++;
        }

        await RefreshIndexAsync(
            targetIndexName,
            cancellationToken);

        await SwitchAliasAsync(
            targetIndexName,
            cancellationToken);

        return count;
    }

private async Task<string> GetNextIndexNameAsync(
    CancellationToken cancellationToken)
{
    var response =
        await _client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.GET,
            $"/_alias/{ActiveAlias}",
            PostData.Empty,
            cancellationToken);

    if (!response.ApiCallDetails.HasSuccessfulStatusCode)
    {
        throw new InvalidOperationException(
            $"Failed to resolve RAG alias '{ActiveAlias}'. " +
            $"StatusCode: " +
            $"{response.ApiCallDetails.HttpStatusCode}. " +
            $"Response: {response.Body}");
    }

    using var document =
        JsonDocument.Parse(response.Body);

    var highestVersion = 1;

    foreach (var property in document.RootElement.EnumerateObject())
    {
        var indexName = property.Name;

        const string prefix = "rag-chunks-v";

        if (!indexName.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        var versionText =
            indexName[prefix.Length..];

        if (int.TryParse(
                versionText,
                out var version))
        {
            highestVersion =
                Math.Max(
                    highestVersion,
                    version);
        }
    }

    return $"rag-chunks-v{highestVersion + 1}";
}
    private async Task CreateTargetIndexAsync(
        string targetIndexName,
        CancellationToken cancellationToken)
    {
        var existsResponse =
            await _client.Indices.ExistsAsync(
                targetIndexName,
                cancellationToken);

        if (existsResponse.Exists)
        {
            throw new InvalidOperationException(
                $"Target index '{targetIndexName}' already exists.");
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
                    Dims = EmbeddingDimensions,
                    Index = true,
                    Similarity = "cosine"
                }
            }
        };

        var response =
            await _client.Indices.CreateAsync(
                targetIndexName,
                descriptor => descriptor
                    .Mappings(mappings => mappings
                        .Properties(properties)),
                cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to create Elasticsearch index " +
                $"'{targetIndexName}'.");
        }
    }

    private async Task<IReadOnlyList<RagDocumentChunk>>
        ReadExistingChunksAsync(
            CancellationToken cancellationToken)
    {
        var response =
            await _client.Transport.RequestAsync<StringResponse>(
                Elastic.Transport.HttpMethod.POST,
                $"/{ActiveAlias}/_search",
                PostData.String("""
                    {
                      "size": 10000,
                      "_source": [
                        "id",
                        "documentId",
                        "content",
                        "chunkIndex",
                        "tenantId",
                        "source"
                      ],
                      "query": {
                        "match_all": {}
                      }
                    }
                    """),
                cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new InvalidOperationException(
                $"Failed to read existing RAG chunks. " +
                $"StatusCode: " +
                $"{response.ApiCallDetails.HttpStatusCode}. " +
                $"DebugInformation: " +
                $"{response.ApiCallDetails.DebugInformation}. " +
                $"Response: {response.Body}");
        }

        using var document =
            JsonDocument.Parse(response.Body);

        if (!document.RootElement.TryGetProperty(
                "hits",
                out var hitsContainer) ||
            !hitsContainer.TryGetProperty(
                "hits",
                out var hits))
        {
            return Array.Empty<RagDocumentChunk>();
        }

        var chunks =
            new List<RagDocumentChunk>();

        foreach (var hit in hits.EnumerateArray())
        {
            if (!hit.TryGetProperty(
                    "_source",
                    out var source))
            {
                continue;
            }

            chunks.Add(
                new RagDocumentChunk
                {
                    Id =
                        source.GetProperty(
                            "id")
                        .GetGuid(),

                    DocumentId =
                        source.GetProperty(
                            "documentId")
                        .GetGuid(),

                    Content =
                        source.GetProperty(
                            "content")
                        .GetString()
                        ?? string.Empty,

                    ChunkIndex =
                        source.GetProperty(
                            "chunkIndex")
                        .GetInt32(),

                    TenantId =
                        source.GetProperty(
                            "tenantId")
                        .GetString()
                        ?? string.Empty,

                    Source =
                        source.GetProperty(
                            "source")
                        .GetString()
                        ?? string.Empty
                });
        }

        return chunks;
    }

    private async Task IndexChunkAsync(
        RagDocumentChunk chunk,
        string targetIndexName,
        CancellationToken cancellationToken)
    {
        var indexDocument = new
        {
            id = chunk.Id,
            documentId = chunk.DocumentId,
            content = chunk.Content,
            chunkIndex = chunk.ChunkIndex,
            tenantId = chunk.TenantId,
            source = chunk.Source,
            embedding = chunk.Embedding
        };

        var response =
            await _client.IndexAsync(
                indexDocument,
                targetIndexName,
                cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to index re-embedded RAG chunk " +
                $"'{chunk.Id}'.");
        }
    }

    private async Task RefreshIndexAsync(
        string indexName,
        CancellationToken cancellationToken)
    {
        var response =
            await _client.Transport.RequestAsync<StringResponse>(
                Elastic.Transport.HttpMethod.POST,
                $"/{indexName}/_refresh",
                PostData.Empty,
                cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new InvalidOperationException(
                $"Failed to refresh RAG index '{indexName}'. " +
                $"StatusCode: " +
                $"{response.ApiCallDetails.HttpStatusCode}.");
        }
    }

    private async Task SwitchAliasAsync(
        string targetIndexName,
        CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            actions = new object[]
            {
                new
                {
                    remove = new
                    {
                        alias = ActiveAlias,
                        index = "*"
                    }
                },
                new
                {
                    add = new
                    {
                        alias = ActiveAlias,
                        index = targetIndexName,
                        is_write_index = true
                    }
                }
            }
        };

        var requestJson =
            JsonSerializer.Serialize(requestBody);

        var response =
            await _client.Transport.RequestAsync<StringResponse>(
                Elastic.Transport.HttpMethod.POST,
                "/_aliases",
                PostData.String(requestJson),
                cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new InvalidOperationException(
                $"Failed to switch RAG alias '{ActiveAlias}' " +
                $"to '{targetIndexName}'. " +
                $"StatusCode: " +
                $"{response.ApiCallDetails.HttpStatusCode}. " +
                $"Response: {response.Body}");
        }
    }
}