namespace Enterprise.InventoryService.Domain.Entities;

public class Inventory
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid TenantId { get; set; }

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public string Warehouse { get; set; } = string.Empty;

    public int ReorderLevel { get; set; }

    public DateTime CreatedOnUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime? ModifiedOnUtc { get; set; }
}