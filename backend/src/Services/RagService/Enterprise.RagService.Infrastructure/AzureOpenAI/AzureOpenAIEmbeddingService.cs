using Azure;
using Azure.AI.OpenAI;
using Enterprise.RagService.Application.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;

namespace Enterprise.RagService.Infrastructure.AzureOpenAI;

public class AzureOpenAIEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _embeddingClient;

    public AzureOpenAIEmbeddingService(
        IOptions<AzureOpenAIOptions> options)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            throw new InvalidOperationException(
                "Azure OpenAI endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException(
                "Azure OpenAI API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.EmbeddingDeployment))
        {
            throw new InvalidOperationException(
                "Azure OpenAI embedding deployment is not configured.");
        }

        var azureClient = new AzureOpenAIClient(
            new Uri(settings.Endpoint),
            new AzureKeyCredential(settings.ApiKey));

        _embeddingClient =
            azureClient.GetEmbeddingClient(
                settings.EmbeddingDeployment);
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Text cannot be empty.",
                nameof(text));
        }

        var response =
            await _embeddingClient.GenerateEmbeddingAsync(
                text,
                cancellationToken: cancellationToken);

        return response.Value.ToFloats().ToArray();
    }
}