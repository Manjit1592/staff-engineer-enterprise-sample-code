namespace Enterprise.InventoryService.Application.DTOs;

public class ReserveStockResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }
}