using Enterprise.InventoryService.Application.Interfaces;
using Enterprise.InventoryService.Domain.Entities;
using Enterprise.InventoryService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.InventoryService.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(
        InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByProductAsync(
        Guid tenantId,
        Guid productId)
    {
        return await _context.Inventories
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.ProductId == productId);
    }

    public async Task AddAsync(
        Inventory inventory)
    {
        await _context.Inventories.AddAsync(inventory);
    }

    public Task UpdateAsync(
        Inventory inventory)
    {
        _context.Inventories.Update(inventory);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}