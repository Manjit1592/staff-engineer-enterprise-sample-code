using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Enterprise.RagService.Application.Interfaces;

namespace Enterprise.RagService.Persistence.Repositories;

public class ElasticsearchRagIndexLifecycleService
    : IRagIndexLifecycleService
{
    private const string ActiveAlias =
        RagIndexNames.ActiveAlias;

    private const string IndexPrefix =
        "rag-chunks-v";

    private readonly ElasticsearchClient _client;

    public ElasticsearchRagIndexLifecycleService(
        ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<int> CleanupOldIndexesAsync(
        CancellationToken cancellationToken = default)
    {
        var activeIndex =
            await GetActiveIndexAsync(
                cancellationToken);

        var indexes =
            await GetVersionedIndexesAsync(
                cancellationToken);

        var orderedIndexes =
            indexes
                .Where(index =>
                    !string.Equals(
                        index,
                        activeIndex,
                        StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(GetVersion)
                .ToList();

        // Keep the newest non-active index as the rollback target.
        var rollbackIndex =
            orderedIndexes.FirstOrDefault();

        var deletedCount = 0;

        foreach (var index in orderedIndexes.Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.Equals(
                    index,
                    rollbackIndex,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await DeleteIndexAsync(
                index,
                cancellationToken);

            deletedCount++;
        }

        return deletedCount;
    }

    private async Task<string> GetActiveIndexAsync(
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
                $"Failed to resolve active RAG alias " +
                $"'{ActiveAlias}'. " +
                $"StatusCode: " +
                $"{response.ApiCallDetails.HttpStatusCode}. " +
                $"Response: {response.Body}");
        }

        using var document =
            JsonDocument.Parse(response.Body);

        var indexes =
            document.RootElement
                .EnumerateObject()
                .Select(property => property.Name)
                .ToList();

        if (indexes.Count != 1)
        {
            throw new InvalidOperationException(
                $"Expected alias '{ActiveAlias}' to point " +
                $"to exactly one index, but found " +
                $"{indexes.Count}.");
        }

        return indexes[0];
    }

    private async Task<IReadOnlyList<string>>
    GetVersionedIndexesAsync(
        CancellationToken cancellationToken)
{
    var response =
        await _client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.GET,
            "/_resolve/index/rag-chunks-v*",
            PostData.Empty,
            cancellationToken);

    if (!response.ApiCallDetails.HasSuccessfulStatusCode)
    {
        throw new InvalidOperationException(
            "Failed to retrieve RAG indexes. " +
            $"StatusCode: " +
            $"{response.ApiCallDetails.HttpStatusCode}. " +
            $"Response: {response.Body}");
    }

    using var document =
        JsonDocument.Parse(response.Body);

    if (!document.RootElement.TryGetProperty(
            "indices",
            out var indices))
    {
        return Array.Empty<string>();
    }

    var result =
        new List<string>();

    foreach (var index in indices.EnumerateArray())
    {
        if (!index.TryGetProperty(
                "name",
                out var nameElement))
        {
            continue;
        }

        var indexName =
            nameElement.GetString();

        if (string.IsNullOrWhiteSpace(indexName))
        {
            continue;
        }

        if (indexName.StartsWith(
                IndexPrefix,
                StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(
                indexName[IndexPrefix.Length..],
                out _))
        {
            result.Add(indexName);
        }
    }

    return result;
}
    private async Task DeleteIndexAsync(
        string indexName,
        CancellationToken cancellationToken)
    {
        var response =
            await _client.Transport.RequestAsync<StringResponse>(
                Elastic.Transport.HttpMethod.DELETE,
                $"/{indexName}",
                PostData.Empty,
                cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new InvalidOperationException(
                $"Failed to delete RAG index '{indexName}'. " +
                $"StatusCode: " +
                $"{response.ApiCallDetails.HttpStatusCode}. " +
                $"Response: {response.Body}");
        }
    }

    private static int GetVersion(
        string indexName)
    {
        return int.TryParse(
            indexName[IndexPrefix.Length..],
            out var version)
            ? version
            : 0;
    }
}