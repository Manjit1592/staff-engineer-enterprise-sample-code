using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure.Mcp;

public class OrderMcpToolExecutor
    : IMcpToolExecutor
{
    private readonly OrderMcpTool _orderTool;

    public OrderMcpToolExecutor(
        OrderMcpTool orderTool)
    {
        _orderTool = orderTool;
    }

    public string ToolName => "get_order";

    public Task<McpToolResponse> ExecuteAsync(
        McpToolExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!context.Arguments.TryGetValue(
                "orderId",
                out var orderIdValue) ||
            string.IsNullOrWhiteSpace(
                orderIdValue?.ToString()))
        {
            throw new ArgumentException(
                "OrderId argument is required.");
        }

        var orderId =
            orderIdValue!.ToString()!;

        var order =
            _orderTool.Execute(
                orderId,
                context.TenantId);

        var response =
            new McpToolResponse
            {
                Success = true,
                ToolName = ToolName,
                Result =
                    $"Order {order.OrderId} is " +
                    $"{order.Status}.",
                Data = order
            };

        return Task.FromResult(response);
    }
}