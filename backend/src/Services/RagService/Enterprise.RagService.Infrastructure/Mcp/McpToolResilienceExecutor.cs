using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;
using Microsoft.Extensions.Logging;

namespace Enterprise.RagService.Infrastructure.Mcp;

public class McpToolResilienceExecutor
{
    private readonly ILogger<McpToolResilienceExecutor> _logger;

    public McpToolResilienceExecutor(
        ILogger<McpToolResilienceExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<McpToolResponse> ExecuteAsync(
        IMcpToolExecutor executor,
        McpToolExecutionContext context,
        McpToolExecutionPolicy policy,
        CancellationToken cancellationToken = default)
    {
        if (executor is null)
        {
            throw new ArgumentNullException(nameof(executor));
        }

        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (policy is null)
        {
            throw new ArgumentNullException(nameof(policy));
        }

        var attempts =
            policy.EnableRetry
                ? policy.MaxRetryCount + 1
                : 1;

        Exception? lastException = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                using var timeoutCts =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken);

                timeoutCts.CancelAfter(
                    TimeSpan.FromSeconds(
                        policy.TimeoutSeconds));

                _logger.LogInformation(
                    "Executing MCP tool {ToolName}. Attempt {Attempt}/{Attempts}.",
                    executor.ToolName,
                    attempt,
                    attempts);

                return await executor.ExecuteAsync(
                    context,
                    timeoutCts.Token);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                lastException =
                    new TimeoutException(
                        $"MCP tool '{executor.ToolName}' timed out.");

                _logger.LogWarning(
                    "MCP tool {ToolName} timed out on attempt {Attempt}.",
                    executor.ToolName,
                    attempt);
            }
            catch (Exception exception)
            {
                lastException = exception;

                _logger.LogWarning(
                    exception,
                    "MCP tool {ToolName} failed on attempt {Attempt}.",
                    executor.ToolName,
                    attempt);
            }

            if (attempt < attempts)
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(100 * attempt),
                    cancellationToken);
            }
        }

        throw lastException
              ?? new InvalidOperationException(
                  $"MCP tool '{executor.ToolName}' failed.");
    }
}