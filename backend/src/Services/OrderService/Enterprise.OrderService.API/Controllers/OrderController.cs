using Enterprise.OrderService.Application.DTOs;
using Enterprise.OrderService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Enterprise.OrderService.API;
using Serilog;

namespace Enterprise.OrderService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public OrderController(
        IOrderService orderService,
        IHubContext<NotificationHub> hubContext)
    {
        _orderService = orderService;
        _hubContext = hubContext;
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost("place")]
    public async Task<IActionResult> PlaceOrder(
        PlaceOrderRequest request)
    {
        Log.Information(
            "Order placement started");

        var result =
            await _orderService.PlaceOrderAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        try
        {
            await _hubContext.Clients.All.SendAsync(
                "OrderPlaced");

            Console.WriteLine(
                "SIGNALR EVENT BROADCASTED");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"SIGNALR ERROR: {ex.Message}");
        }

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("status/{orderId}")]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid orderId,
        UpdateOrderStatusRequest request)
    {
        var result =
            await _orderService.UpdateOrderStatusAsync(
                orderId,
                request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        try
        {
            await _hubContext.Clients.All.SendAsync(
                "OrderStatusUpdated",
                orderId,
                request.Status);

            Console.WriteLine(
                "ORDER STATUS UPDATED EVENT SENT");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"SIGNALR STATUS ERROR: {ex.Message}");
        }

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var result =
            await _orderService.GetAllOrdersAsync();

        return Ok(result);
    }
}