using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Persistence.Repositories;

public class ElasticsearchRagKeywordSearchRepository
    : IRagKeywordSearchRepository
{
    private const string IndexName = RagIndexNames.ActiveAlias;

    private readonly ElasticsearchClient _client;

    public ElasticsearchRagKeywordSearchRepository(
        ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<RagSearchResult>> SearchAsync(
        string query,
        string tenantId,
        RagSearchFilter? filter = null,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException(
                "Query cannot be empty.",
                nameof(query));
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

        var requestBody = new
        {
            size = topK,
            query = new
            {
                @bool = new
                {
                    must = new[]
                    {
                        new
                        {
                            match = new Dictionary<string, object>
                            {
                                ["content"] = query
                            }
                        }
                    },
                    filter = filters
                }
            }
        };

        var requestJson =
            JsonSerializer.Serialize(requestBody);

        var response =
            await _client.Transport.RequestAsync<StringResponse>(
                Elastic.Transport.HttpMethod.POST,
                $"/{IndexName}/_search",
                PostData.String(requestJson),
                cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new InvalidOperationException(
                $"Elasticsearch keyword search failed. " +
                $"StatusCode: {response.ApiCallDetails.HttpStatusCode}. " +
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
                ChunkId =
                    source.GetProperty("id").GetGuid(),

                DocumentId =
                    source.GetProperty("documentId").GetGuid(),

                Content =
                    source.GetProperty("content").GetString()
                    ?? string.Empty,

                ChunkIndex =
                    source.GetProperty("chunkIndex").GetInt32(),

                TenantId =
                    source.GetProperty("tenantId").GetString()
                    ?? string.Empty,

                Score =
                    hit.TryGetProperty("_score", out var score)
                        ? score.GetDouble()
                        : 0
            };

            results.Add(result);
        }

        return results;
    }
}