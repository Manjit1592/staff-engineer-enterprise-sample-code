using Azure;
using Azure.AI.OpenAI;
using Enterprise.RagService.Application.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Enterprise.RagService.Infrastructure.AzureOpenAI;

public class AzureOpenAILlmService : ILlmService
{
    private readonly ChatClient _chatClient;

    public AzureOpenAILlmService(
        IOptions<AzureOpenAIChatOptions> options)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            throw new InvalidOperationException(
                "Azure OpenAI chat endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException(
                "Azure OpenAI chat API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ChatDeployment))
        {
            throw new InvalidOperationException(
                "Azure OpenAI chat deployment is not configured.");
        }

        var azureClient = new AzureOpenAIClient(
            new Uri(settings.Endpoint),
            new AzureKeyCredential(settings.ApiKey));

        _chatClient = azureClient.GetChatClient(
            settings.ChatDeployment);
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

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var response = await _chatClient.CompleteChatAsync(
            messages,
            cancellationToken: cancellationToken);

        return response.Value.Content[0].Text;
    }
}