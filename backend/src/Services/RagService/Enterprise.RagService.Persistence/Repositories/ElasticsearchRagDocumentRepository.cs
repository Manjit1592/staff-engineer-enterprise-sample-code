using Elastic.Clients.Elasticsearch;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Domain.Entities;
using Enterprise.RagService.Persistence.Models;

namespace Enterprise.RagService.Persistence.Repositories;

public class ElasticsearchRagDocumentRepository : IRagDocumentRepository
{
    private const string IndexName = "rag-documents";

    private readonly ElasticsearchClient _client;

    public ElasticsearchRagDocumentRepository(
        ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task AddAsync(
        RagDocument document,
        CancellationToken cancellationToken = default)
    {
        var indexDocument = new RagDocumentIndex
        {
            Id = document.Id,
            Title = document.Title,
            Content = document.Content,
            Source = document.Source,
            TenantId = document.TenantId,
            CreatedOnUtc = document.CreatedOnUtc
        };

        await _client.IndexAsync(
            indexDocument,
            IndexName,
            cancellationToken);
    }

    public async Task<RagDocument?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
{
    var response = await _client.GetAsync<RagDocumentIndex>(
        IndexName,
        id,
        cancellationToken);

    if (!response.Found || response.Source is null)
    {
        return null;
    }

    return new RagDocument
    {
        Id = response.Source.Id,
        Title = response.Source.Title,
        Content = response.Source.Content,
        Source = response.Source.Source,
        TenantId = response.Source.TenantId,
        CreatedOnUtc = response.Source.CreatedOnUtc
    };
}
}