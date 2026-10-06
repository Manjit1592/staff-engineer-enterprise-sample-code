namespace Enterprise.RagService.Infrastructure.AzureOpenAI;

public class AzureOpenAIOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string EmbeddingDeployment { get; set; } = string.Empty;
}