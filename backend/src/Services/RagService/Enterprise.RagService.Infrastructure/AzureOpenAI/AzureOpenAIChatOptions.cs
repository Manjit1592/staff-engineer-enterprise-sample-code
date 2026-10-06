namespace Enterprise.RagService.Infrastructure.AzureOpenAI;

public class AzureOpenAIChatOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string ChatDeployment { get; set; } = string.Empty;
}