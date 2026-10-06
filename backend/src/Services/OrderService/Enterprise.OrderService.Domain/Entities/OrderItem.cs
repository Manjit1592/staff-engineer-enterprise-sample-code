using Enterprise.SharedKernel.Entities;
using System.Text.Json.Serialization;

namespace Enterprise.OrderService.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }
}