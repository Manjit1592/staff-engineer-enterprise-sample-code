namespace Enterprise.RagService.Application.Models;

public class RagPrompt
{
    public string SystemPrompt { get; set; } = string.Empty;

    public string UserPrompt { get; set; } = string.Empty;
}