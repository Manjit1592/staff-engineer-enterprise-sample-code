using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure;

public class InventoryMcpTool
{
    public InventoryToolResult Execute(
        string productId,
        string tenantId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException(
                "ProductId cannot be empty.",
                nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException(
                "TenantId cannot be empty.",
                nameof(tenantId));
        }

        return new InventoryToolResult
        {
            ProductId = productId,
            AvailableQuantity = 42,
            Warehouse = "WH-001"
        };
    }
}