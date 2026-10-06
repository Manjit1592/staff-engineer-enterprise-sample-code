namespace Enterprise.EventBus.RabbitMQ.Events;

public class ProductCreatedEvent
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }
}