using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;
using Microsoft.Extensions.Logging;

namespace Enterprise.RagService.Infrastructure;

public class LocalEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly LocalEmbeddingOptions _options;
    private readonly ILogger<LocalEmbeddingService> _logger;

    public LocalEmbeddingService(
        HttpClient httpClient,
        LocalEmbeddingOptions options,
        ILogger<LocalEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;

        _httpClient.BaseAddress =
            new Uri(_options.Endpoint.TrimEnd('/') + "/");

        _httpClient.Timeout =
            TimeSpan.FromSeconds(
                Math.Max(1, _options.TimeoutSeconds));
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

        cancellationToken.ThrowIfCancellationRequested();

        var request = new LocalEmbeddingRequest
        {
            Model = _options.Model,
            Prompt = text
        };

        _logger.LogInformation(
            "Generating local embedding using model {Model}.",
            _options.Model);

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/embeddings",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<LocalEmbeddingResponse>(
                cancellationToken);

        if (result is null ||
            result.Embedding is null ||
            result.Embedding.Length == 0)
        {
            throw new InvalidOperationException(
                "Local embedding model returned an empty embedding.");
        }

        if (_options.Dimensions > 0 &&
            result.Embedding.Length != _options.Dimensions)
        {
            throw new InvalidOperationException(
                $"Embedding dimension mismatch. " +
                $"Expected {_options.Dimensions}, " +
                $"received {result.Embedding.Length}.");
        }

        return result.Embedding;
    }

    private sealed class LocalEmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;
    }

    private sealed class LocalEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = [];
    }
}