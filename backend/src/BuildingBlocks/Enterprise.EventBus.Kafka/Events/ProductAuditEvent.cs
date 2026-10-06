namespace Enterprise.EventBus.Kafka.Events;

public class ProductAuditEvent
{
    public Guid ProductId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime OccurredOnUtc { get; set; }
}