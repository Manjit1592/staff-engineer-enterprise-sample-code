using Enterprise.SharedKernel.Entities;

namespace Enterprise.OrderService.Domain.Entities;

public class OutboxMessage : BaseEntity
{
    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedOnUtc { get; set; }

    public bool IsProcessed { get; set; }

    public string? Error { get; set; }
}