
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure.Mcp;

public class InventoryMcpToolExecutor : IMcpToolExecutor
{
    private readonly InventoryMcpTool _inventoryTool;

    public InventoryMcpToolExecutor(
        InventoryMcpTool inventoryTool)
    {
        _inventoryTool = inventoryTool;
    }

    public string ToolName => "get_inventory";

    public Task<McpToolResponse> ExecuteAsync(
        McpToolExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (string.IsNullOrWhiteSpace(context.TenantId))
        {
            throw new ArgumentException(
                "TenantId cannot be empty.",
                nameof(context.TenantId));
        }

        if (!context.Arguments.TryGetValue(
                "productId",
                out var productIdValue) ||
            string.IsNullOrWhiteSpace(
                productIdValue?.ToString()))
        {
            throw new ArgumentException(
                "ProductId argument is required.");
        }

        var productId =
            productIdValue!.ToString()!;

        var inventory =
            _inventoryTool.Execute(
                productId,
                context.TenantId);

        var response =
            new McpToolResponse
            {
                Success = true,
                ToolName = ToolName,
                Result =
                    $"Product {inventory.ProductId} has " +
                    $"{inventory.AvailableQuantity} units " +
                    $"available at {inventory.Warehouse}.",
                Data = inventory
            };

        return Task.FromResult(response);
    }
}