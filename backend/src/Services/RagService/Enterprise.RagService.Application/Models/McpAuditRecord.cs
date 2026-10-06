namespace Enterprise.RagService.Application.Models;

public class McpAuditRecord
{
    public DateTime TimestampUtc { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string ToolName { get; set; } = string.Empty;

    public bool Success { get; set; }

    public string ErrorCode { get; set; } = string.Empty;

    public long DurationMs { get; set; }
}