namespace Enterprise.Common.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; }
}