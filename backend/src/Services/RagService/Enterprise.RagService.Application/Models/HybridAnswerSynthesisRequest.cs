namespace Enterprise.RagService.Application.Models;

public class HybridAnswerSynthesisRequest
{
    public string Query { get; set; } = string.Empty;

    public string RagAnswer { get; set; } = string.Empty;

    public string McpResult { get; set; } = string.Empty;
}