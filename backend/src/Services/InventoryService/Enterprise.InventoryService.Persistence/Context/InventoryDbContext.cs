using Enterprise.InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.InventoryService.Persistence.Context;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Inventory> Inventories => Set<Inventory>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventory>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Inventory>()
            .Property(x => x.AvailableQuantity)
            .IsRequired();

        modelBuilder.Entity<Inventory>()
            .Property(x => x.ReservedQuantity)
            .IsRequired();

        modelBuilder.Entity<Inventory>()
            .Property(x => x.Warehouse)
            .HasMaxLength(100);

        modelBuilder.Entity<Inventory>()
            .HasIndex(x => x.ProductId);

        modelBuilder.Entity<Inventory>()
            .HasIndex(x => x.TenantId);

        modelBuilder.Entity<Inventory>()
            .HasIndex(x => new
            {
                x.TenantId,
                x.ProductId
            })
            .IsUnique();
    }
}