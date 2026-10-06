namespace Enterprise.RagService.Application.Interfaces;

public interface ILlmService
{
    Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}