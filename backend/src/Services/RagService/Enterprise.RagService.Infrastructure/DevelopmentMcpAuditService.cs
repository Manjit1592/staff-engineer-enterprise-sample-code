using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;
using Microsoft.Extensions.Logging;

namespace Enterprise.RagService.Infrastructure;

public class DevelopmentMcpAuditService
    : IMcpAuditService
{
    private readonly ILogger<DevelopmentMcpAuditService> _logger;

    public DevelopmentMcpAuditService(
        ILogger<DevelopmentMcpAuditService> logger)
    {
        _logger = logger;
    }

    public Task RecordAsync(
        McpAuditRecord record,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MCP audit. CorrelationId: {CorrelationId}, " +
            "TenantId: {TenantId}, UserId: {UserId}, " +
            "Role: {Role}, ToolName: {ToolName}, " +
            "Success: {Success}, ErrorCode: {ErrorCode}, " +
            "DurationMs: {DurationMs}",
            record.CorrelationId,
            record.TenantId,
            record.UserId,
            record.Role,
            record.ToolName,
            record.Success,
            record.ErrorCode,
            record.DurationMs);

        return Task.CompletedTask;
    }
}