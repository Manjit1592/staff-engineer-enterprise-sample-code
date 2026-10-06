namespace Enterprise.ProductService.Infrastructure.Clients;

public interface IInventoryClient
{
    Task<int?> GetAvailableQuantityAsync(
        Guid productId,
        Guid tenantId);
}