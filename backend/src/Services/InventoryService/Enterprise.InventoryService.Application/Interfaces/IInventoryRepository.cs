using Enterprise.InventoryService.Domain.Entities;

namespace Enterprise.InventoryService.Application.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductAsync(
        Guid tenantId,
        Guid productId);

    Task UpdateAsync(
        Inventory inventory);

    Task AddAsync(
        Inventory inventory);

    Task SaveChangesAsync();
}