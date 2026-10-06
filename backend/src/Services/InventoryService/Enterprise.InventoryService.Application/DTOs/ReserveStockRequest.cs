namespace Enterprise.InventoryService.Application.DTOs;

public class ReserveStockRequest
{
    public Guid ProductId { get; set; }

    public Guid TenantId { get; set; }

    public int Quantity { get; set; }

    public Guid OrderId { get; set; }
}