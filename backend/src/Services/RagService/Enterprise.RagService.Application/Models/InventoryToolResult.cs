namespace Enterprise.RagService.Application.Models;

public class InventoryToolResult
{
    public string ProductId { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }

    public string Warehouse { get; set; } = string.Empty;

    public bool IsAvailable => AvailableQuantity > 0;
}