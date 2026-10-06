namespace Enterprise.RagService.Application.Models;

public class McpToolExecutionPolicy
{
    public int TimeoutSeconds { get; set; } = 10;

    public int MaxRetryCount { get; set; } = 2;

    public bool EnableRetry { get; set; } = true;
}