namespace Enterprise.OrderService.Application.DTOs;

public class PlaceOrderRequest
{
    public Guid UserId { get; set; }

    public List<PlaceOrderItemRequest> Items { get; set; } = new();
}