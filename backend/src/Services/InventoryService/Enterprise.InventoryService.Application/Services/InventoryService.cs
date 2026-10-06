using Enterprise.InventoryService.Application.DTOs;
using Enterprise.InventoryService.Application.Interfaces;

namespace Enterprise.InventoryService.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;

    public InventoryService(
        IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReserveStockResponse> ReserveStockAsync(
        ReserveStockRequest request)
    {
        var inventory =
            await _repository.GetByProductAsync(
                request.TenantId,
                request.ProductId);

        if (inventory == null)
        {
            return new ReserveStockResponse
            {
                Success = false,
                Message = "Inventory not found."
            };
        }

        if (inventory.AvailableQuantity < request.Quantity)
        {
            return new ReserveStockResponse
            {
                Success = false,
                Message = "Insufficient stock.",
                AvailableQuantity = inventory.AvailableQuantity,
                ReservedQuantity = inventory.ReservedQuantity
            };
        }

        inventory.AvailableQuantity -= request.Quantity;

        inventory.ReservedQuantity += request.Quantity;

        await _repository.UpdateAsync(inventory);

        await _repository.SaveChangesAsync();

        return new ReserveStockResponse
        {
            Success = true,
            Message = "Stock reserved successfully.",
            AvailableQuantity = inventory.AvailableQuantity,
            ReservedQuantity = inventory.ReservedQuantity
        };
    }

public async Task<int> GetAvailableQuantityAsync(
    Guid tenantId,
    Guid productId)
{
    var inventory =
        await _repository.GetByProductAsync(
            tenantId,
            productId);

    if (inventory == null)
    {
        return 0;
    }

    return inventory.AvailableQuantity;
}
}