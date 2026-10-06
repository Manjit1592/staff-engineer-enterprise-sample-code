using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;
using Enterprise.RagService.Persistence.Models;

namespace Enterprise.RagService.Persistence.Repositories;

public class ElasticsearchRagVectorSearchRepository
    : IRagVectorSearchRepository
{
    private const string IndexName = "rag-chunks-v2";

    private readonly ElasticsearchClient _client;

    public ElasticsearchRagVectorSearchRepository(
        ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<RagSearchResult>> SearchAsync(
        float[] queryEmbedding,
        string tenantId,
        RagSearchFilter? filter = null,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (queryEmbedding.Length != 768)
        {
            throw new ArgumentException(
                "Query embedding must contain exactly 128 dimensions.",
                nameof(queryEmbedding));
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException(
                "TenantId cannot be empty.",
                nameof(tenantId));
        }

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topK));
        }

        var filters = new List<object>
        {
            new
            {
                term = new Dictionary<string, object>
                {
                    ["tenantId"] = tenantId
                }
            }
        };

        if (!string.IsNullOrWhiteSpace(filter?.Source))
        {
            filters.Add(
                new
                {
                    term = new Dictionary<string, object>
                    {
                        ["source"] = filter.Source
                    }
                });
        }

        if (!string.IsNullOrWhiteSpace(filter?.DocumentType))
        {
            filters.Add(
                new
                {
                    term = new Dictionary<string, object>
                    {
                        ["documentType"] = filter.DocumentType
                    }
                });
        }

        var requestBody = new
        {
            knn = new
            {
                field = "embedding",
                query_vector = queryEmbedding,
                k = topK,
                num_candidates = Math.Max(topK * 10, 50),
                filter = new
                {
                    @bool = new
                    {
                        filter = filters
                    }
                }
            }
        };

        var requestJson =
            JsonSerializer.Serialize(requestBody);

        StringResponse response;

try
{
    response =
        await _client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.POST,
            $"/{IndexName}/_search",
            PostData.String(requestJson),
            cancellationToken);
}
catch (Exception exception)
{
    throw new InvalidOperationException(
        "Elasticsearch vector search transport request failed.",
        exception);
}

if (!response.ApiCallDetails.HasSuccessfulStatusCode)
{
    throw new InvalidOperationException(
        $"Elasticsearch vector search failed. " +
        $"StatusCode: {response.ApiCallDetails.HttpStatusCode}. " +
        $"DebugInformation: {response.ApiCallDetails.DebugInformation}. " +
        $"Response: {response.Body}");
}

        using var document =
            JsonDocument.Parse(response.Body);

        if (!document.RootElement.TryGetProperty(
                "hits",
                out var hitsElement))
        {
            return Array.Empty<RagSearchResult>();
        }

        if (!hitsElement.TryGetProperty(
                "hits",
                out var hits))
        {
            return Array.Empty<RagSearchResult>();
        }

        var results = new List<RagSearchResult>();

        foreach (var hit in hits.EnumerateArray())
        {
            if (!hit.TryGetProperty(
                    "_source",
                    out var source))
            {
                continue;
            }

            var result = new RagSearchResult
            {
                ChunkId = source.GetProperty("id").GetGuid(),
                DocumentId = source.GetProperty("documentId").GetGuid(),
                Content = source.GetProperty("content").GetString()
                    ?? string.Empty,
                ChunkIndex = source.GetProperty("chunkIndex").GetInt32(),
                TenantId = source.GetProperty("tenantId").GetString()
                    ?? string.Empty,
                Score = hit.TryGetProperty("_score", out var score)
                    ? score.GetDouble()
                    : 0
            };

            results.Add(result);
        }

        return results;
    }
}