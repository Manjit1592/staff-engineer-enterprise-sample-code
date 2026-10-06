namespace Enterprise.EventBus.Kafka.Events;

public class OrderAuditEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OccurredOnUtc { get; set; }
}