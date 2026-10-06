using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IRagPromptBuilder
{
    RagPrompt Build(
        string query,
        RagContext context);
}