using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure.Mcp;

public class OrderMcpTool
{
    public OrderToolResult Execute(
        string orderId,
        string tenantId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException(
                "OrderId cannot be empty.",
                nameof(orderId));
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException(
                "TenantId cannot be empty.",
                nameof(tenantId));
        }

        return new OrderToolResult
        {
            OrderId = orderId,
            Status = "Confirmed",
            TenantId = tenantId
        };
    }
}