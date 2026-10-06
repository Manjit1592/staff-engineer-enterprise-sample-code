namespace Enterprise.OrderService.Infrastructure.Clients;

public interface IInventoryClient
{
    Task<bool> ReserveStockAsync(
        Guid productId,
        Guid tenantId,
        int quantity,
        Guid orderId);
}