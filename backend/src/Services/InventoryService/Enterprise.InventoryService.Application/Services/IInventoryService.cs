using Enterprise.InventoryService.Application.DTOs;

namespace Enterprise.InventoryService.Application.Services;

public interface IInventoryService
{
    Task<ReserveStockResponse> ReserveStockAsync(
        ReserveStockRequest request);

    Task<int> GetAvailableQuantityAsync(
        Guid tenantId,
        Guid productId);
}