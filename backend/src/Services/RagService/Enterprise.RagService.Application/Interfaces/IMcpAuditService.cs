using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IMcpAuditService
{
    Task RecordAsync(
        McpAuditRecord record,
        CancellationToken cancellationToken = default);
}