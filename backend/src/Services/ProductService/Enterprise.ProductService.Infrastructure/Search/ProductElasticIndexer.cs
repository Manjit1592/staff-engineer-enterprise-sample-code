using Enterprise.ProductService.Domain.Entities;
using Nest;

namespace Enterprise.ProductService.Infrastructure.Search;

public class ProductElasticIndexer
{
    private readonly ElasticClient _client;

    public ProductElasticIndexer()
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("products");

        _client = new ElasticClient(settings);
    }

    public async Task IndexProductAsync(Product product)
    {
        await _client.IndexDocumentAsync(product);
    }

    public async Task<List<Product>> SearchProductsAsync(string term)
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f
                        .Field(p => p.Name)
                        .Field(p => p.Description)
                        .Field(p => p.SKU))
                    .Query(term))));

        return response.Documents.ToList();
    }
}