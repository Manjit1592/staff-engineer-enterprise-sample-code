using System.Diagnostics;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;
using Enterprise.RagService.Infrastructure.Mcp;

namespace Enterprise.RagService.Infrastructure;

public class DevelopmentMcpToolService : IMcpToolService
{
    private readonly IMcpToolRegistry _toolRegistry;
    private readonly IMcpToolArgumentValidator _argumentValidator;
    private readonly IMcpAuthorizationService _authorizationService;
    private readonly IMcpFailureHandler _failureHandler;
    private readonly IMcpAuditService _auditService;
    private readonly DevelopmentMcpToolExecutorRegistry _executorRegistry;
    private readonly McpToolResilienceExecutor _resilienceExecutor;

    public DevelopmentMcpToolService(
        IMcpToolRegistry toolRegistry,
        IMcpToolArgumentValidator argumentValidator,
        IMcpAuthorizationService authorizationService,
        IMcpFailureHandler failureHandler,
        IMcpAuditService auditService,
        DevelopmentMcpToolExecutorRegistry executorRegistry,
        McpToolResilienceExecutor resilienceExecutor)
    {
        _toolRegistry = toolRegistry;
        _argumentValidator = argumentValidator;
        _authorizationService = authorizationService;
        _failureHandler = failureHandler;
        _auditService = auditService;
        _executorRegistry = executorRegistry;
        _resilienceExecutor = resilienceExecutor;
    }

    public async Task<McpToolResponse> ExecuteAsync(
        McpToolRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(
                request.CorrelationId))
        {
            request.CorrelationId =
                Guid.NewGuid().ToString("N");
        }

        var stopwatch = Stopwatch.StartNew();

        var response =
            new McpToolResponse
            {
                Success = false,
                ToolName = request.ToolName
            };

        try
        {
            if (string.IsNullOrWhiteSpace(
                    request.ToolName))
            {
                response =
                    _failureHandler.Handle(
                        request.ToolName,
                        "INVALID_TOOL",
                        "ToolName cannot be empty.");

                return response;
            }

            if (string.IsNullOrWhiteSpace(
                    request.TenantId))
            {
                response =
                    _failureHandler.Handle(
                        request.ToolName,
                        "UNAUTHORIZED",
                        "TenantId is required.");

                return response;
            }

            if (string.IsNullOrWhiteSpace(
                    request.UserId))
            {
                response =
                    _failureHandler.Handle(
                        request.ToolName,
                        "UNAUTHENTICATED",
                        "UserId is required.");

                return response;
            }

            if (string.IsNullOrWhiteSpace(
                    request.Role))
            {
                response =
                    _failureHandler.Handle(
                        request.ToolName,
                        "UNAUTHORIZED",
                        "Role is required.");

                return response;
            }

            var tool =
                _toolRegistry.GetTool(
                    request.ToolName);

            if (tool is null)
            {
                response =
                    _failureHandler.Handle(
                        request.ToolName,
                        "TOOL_NOT_FOUND",
                        "The requested MCP tool was not found.");

                return response;
            }

            var authorization =
                _authorizationService.Authorize(
                    request.TenantId,
                    request.Role,
                    tool);

            if (!authorization.IsAllowed)
            {
                response =
                    _failureHandler.Handle(
                        tool.Name,
                        "UNAUTHORIZED",
                        authorization.Reason);

                return response;
            }

            var validation =
                _argumentValidator.Validate(
                    tool,
                    request.Arguments);

            if (!validation.IsValid)
            {
                response =
                    _failureHandler.Handle(
                        tool.Name,
                        "INVALID_ARGUMENTS",
                        validation.Error);

                return response;
            }

            var executor =
                _executorRegistry.GetExecutor(
                    tool.Name);

            if (executor is null)
            {
                response =
                    _failureHandler.Handle(
                        tool.Name,
                        "EXECUTOR_NOT_FOUND",
                        "No executor is registered for the requested MCP tool.");

                return response;
            }

            response =
                await _resilienceExecutor.ExecuteAsync(
                    executor,
                    new McpToolExecutionContext
                    {
                        TenantId = request.TenantId,
                        Query = request.Query,
                        Arguments = request.Arguments
                    },
                    tool.ExecutionPolicy,
                    cancellationToken);

            return response;
        }
        catch (OperationCanceledException)
        {
            response =
                _failureHandler.Handle(
                    request.ToolName,
                    "TOOL_CANCELLED",
                    "The MCP tool execution was cancelled.");

            return response;
        }
        catch (Exception)
        {
            response =
                _failureHandler.Handle(
                    request.ToolName,
                    "TOOL_EXECUTION_FAILED",
                    "The MCP tool execution failed.");

            return response;
        }
        finally
        {
            stopwatch.Stop();

            var failure =
                response.Data as McpToolFailure;

            await _auditService.RecordAsync(
                new McpAuditRecord
                {
                    TimestampUtc =
                        DateTime.UtcNow,

                    CorrelationId =
                        request.CorrelationId,

                    TenantId =
                        request.TenantId,

                    UserId =
                        request.UserId,

                    Role =
                        request.Role,

                    ToolName =
                        request.ToolName,

                    Success =
                        response.Success,

                    ErrorCode =
                        failure?.ErrorCode
                        ?? string.Empty,

                    DurationMs =
                        stopwatch.ElapsedMilliseconds
                },
                CancellationToken.None);
        }
    }
}