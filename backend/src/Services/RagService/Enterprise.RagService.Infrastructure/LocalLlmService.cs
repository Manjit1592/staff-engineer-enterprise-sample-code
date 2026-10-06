using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;
using Microsoft.Extensions.Logging;

namespace Enterprise.RagService.Infrastructure;

public class LocalLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly LocalLlmOptions _options;
    private readonly ILogger<LocalLlmService> _logger;

    public LocalLlmService(
        HttpClient httpClient,
        LocalLlmOptions options,
        ILogger<LocalLlmService> logger)
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

    public async Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            throw new ArgumentException(
                "System prompt cannot be empty.",
                nameof(systemPrompt));
        }

        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            throw new ArgumentException(
                "User prompt cannot be empty.",
                nameof(userPrompt));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var request = new LocalLlmRequest
        {
            Model = _options.Model,
            System = systemPrompt,
            Prompt = userPrompt,
            Stream = false,
            Options = new LocalLlmGenerationOptions
            {
                Temperature = _options.Temperature
            }
        };

        _logger.LogInformation(
            "Sending RAG request to local LLM model {Model}.",
            _options.Model);

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/generate",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<LocalLlmResponse>(
                cancellationToken);

        if (result is null ||
            string.IsNullOrWhiteSpace(result.Response))
        {
            throw new InvalidOperationException(
                "Local LLM returned an empty response.");
        }

        return result.Response.Trim();
    }

    private sealed class LocalLlmRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("system")]
        public string System { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("options")]
        public LocalLlmGenerationOptions Options { get; set; } = new();
    }

    private sealed class LocalLlmGenerationOptions
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
    }

    private sealed class LocalLlmResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;
    }
}