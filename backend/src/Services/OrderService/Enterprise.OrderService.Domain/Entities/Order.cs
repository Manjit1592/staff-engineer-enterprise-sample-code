using Enterprise.SharedKernel.Entities;
using Enterprise.Common.Interfaces;

namespace Enterprise.OrderService.Domain.Entities;

public class Order : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "PENDING";

    public ICollection<OrderItem> Items { get; set; } =
        new List<OrderItem>();
}