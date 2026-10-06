using Enterprise.RagService.Application.Interfaces;

namespace Enterprise.RagService.Infrastructure.AzureOpenAI;

public class DevelopmentLlmService : ILlmService
{
    public Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            throw new ArgumentException(
                "User prompt cannot be empty.",
                nameof(userPrompt));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var answer =
            "Development LLM response.\n\n" +
            "The following context was provided to the LLM:\n\n" +
            userPrompt;

        return Task.FromResult(answer);
    }
}